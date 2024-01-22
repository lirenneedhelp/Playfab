using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.GroupsModels;
using TMPro;

public class GuildTestController : MonoBehaviour
{
    [SerializeField]
    GameObject guildCreatePanel;

    [SerializeField]
    GameObject guild_content, guild_content_prefab;

    List<PlayFab.ClientModels.GetUserDataResult> resultList = new();
    List<string> GuildNames = new();
    // A local cache of some bits of PlayFab data
    // This cache pretty much only serves this example , and assumes that entities are uniquely identifiable by EntityId alone, which isn't technically true. Your data cache will have to be better.
    public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    public static EntityKey EntityKeyMaker(string entityId)
    {
        return new EntityKey { Id = entityId };
    }

    private void OnSharedError(PlayFab.PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    public void ListGroups()
    {
        var req = new PlayFab.AdminModels.GetPlayersInSegmentRequest() { SegmentId = "7D1D5E303AB2D17" };
        PlayFabAdminAPI.GetPlayersInSegment(req, result =>
        {
            var list = result.PlayerProfiles;
            resultList.Clear();
            foreach (PlayFab.AdminModels.PlayerProfile profile in list)
            {
                if (!string.IsNullOrEmpty(profile.DisplayName))
                {
                    //Debug.Log(profile.DisplayName);

                    var req = new PlayFab.ClientModels.GetUserDataRequest() { PlayFabId = profile.PlayerId };
                    PlayFabClientAPI.GetUserData(req,
                        result =>
                        {
                            resultList.Add(result);

                        }, error => { Debug.Log(error.GenerateErrorReport()); });
                }

            }

            StartCoroutine(GuildListResults());

        }, error =>
        {
            Debug.Log("Idk tbh");
        }
        );
       
    }
    private IEnumerator GuildListResults()
    {
        yield return new WaitForSeconds(1f); 

        foreach (var r in resultList)
        {
            if (r.Data != null && r.Data.ContainsKey("entityID") && r.Data.ContainsKey("entityType"))
            {
                Debug.Log(r.Data["entityID"].Value);
                Debug.Log(r.Data["entityType"].Value);

                // Do something with entityID and entityType
                var request = new ListMembershipRequest
                {
                    Entity = new EntityKey { Id = r.Data["entityID"].Value, Type = r.Data["entityType"].Value }
                };
                PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
            }
        }
        StartCoroutine(ListGuilds());

    }
    private void OnListGroups(ListMembershipResponse response)
    {
        var prevRequest = (ListMembershipRequest)response.Request;
        Debug.Log("Finding Guild List");

        foreach (var pair in response.Groups)
        {
            Debug.Log("Guild Found");

            GroupNameById[pair.Group.Id] = pair.GroupName;
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));
            GuildNames.Add(pair.GroupName);

        }
    }

    public IEnumerator ListGuilds()
    {
        yield return new WaitForSeconds(1f); 

        foreach (Transform child in guild_content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var pair in GuildNames)
        {
            GameObject newGuildContent = Instantiate(guild_content_prefab, guild_content.transform);

            // Assuming there is a Text component in your prefab, set its text to the group name
            TMP_Text textComponent = newGuildContent.transform.Find("GuildNameText").GetComponent<TMP_Text>();
            textComponent.text = pair;
        }
    }

    public void CreateGroup()
    {
    //string groupName, EntityKey entityKey
        Debug.Log("Creating Guild");
        Debug.Log(guildCreatePanel.transform.Find("GuildNameInput").GetComponent<TMP_InputField>().text);
        // A player-controlled entity creates a new group
        var request = new CreateGroupRequest { GroupName = guildCreatePanel.transform.Find("GuildNameInput").GetComponent<TMP_InputField>().text };
        PlayFabGroupsAPI.CreateGroup(request, OnCreateGroup, OnSharedError);
    }
    private void OnCreateGroup(CreateGroupResponse response)
    {
        Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);
        var req = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string> {
                { "entityID", DataCarrier.Instance.entityID },
                { "entityType", DataCarrier.Instance.entityType }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, result => { Debug.Log("Updated Guild Leader"); }, error => { Debug.Log("Failed to update guild leader"); });

        var prevRequest = (CreateGroupRequest)response.Request;
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, response.Group.Id));
        GroupNameById[response.Group.Id] = response.GroupName;
    }
    public void DeleteGroup(string groupId)
    {
        // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
        var request = new DeleteGroupRequest { Group = EntityKeyMaker(groupId) };
        PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
    }
    private void OnDeleteGroup(EmptyResponse response)
    {
        var prevRequest = (DeleteGroupRequest)response.Request;
        Debug.Log("Group Deleted: " + prevRequest.Group.Id);

        var temp = new HashSet<KeyValuePair<string, string>>();
        foreach (var each in EntityGroupPairs)
            if (each.Value != prevRequest.Group.Id)
                temp.Add(each);
        EntityGroupPairs.IntersectWith(temp);
        GroupNameById.Remove(prevRequest.Group.Id);
    }

    public void InviteToGroup(string groupId, EntityKey entityKey)
    {
        // A player-controlled entity invites another player-controlled entity to an existing group
        var request = new InviteToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.InviteToGroup(request, OnInvite, OnSharedError);
    }
    public void OnInvite(InviteToGroupResponse response)
    {
        var prevRequest = (InviteToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id), Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);
    }
    public void OnAcceptInvite(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupInvitationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
        EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, prevRequest.Group.Id));
    }

    public void ApplyToGroup(string groupId, EntityKey entityKey)
    {
        // A player-controlled entity applies to join an existing group (of which they are not already a member)
        var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.ApplyToGroup(request, OnApply, OnSharedError);
    }
    public void OnApply(ApplyToGroupResponse response)
    {
        var prevRequest = (ApplyToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupApplicationRequest { Group = prevRequest.Group, Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
    }
    public void OnAcceptApplication(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupApplicationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
    }
    public void KickMember(string groupId, EntityKey entityKey)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
    }
    private void OnKickMembers(EmptyResponse response)
    {
        var prevRequest= (RemoveMembersRequest)response.Request;
            
        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
        EntityGroupPairs.Remove(new KeyValuePair<string, string>(prevRequest.Members[0].Id, prevRequest.Group.Id));
    }

    public void OpenGuildCreatePanel()
    {
        guildCreatePanel.SetActive(true);
    }
    public void CloseGuildCreatePanel()
    {
        guildCreatePanel.SetActive(false);
    }
}
