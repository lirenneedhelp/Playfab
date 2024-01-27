using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.GroupsModels;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class GuildTestController : MonoBehaviourPun
{
    class GuildInfo
    {
        public EntityKey entityKey;
        public string guildName;

        public GuildInfo(EntityKey key, string name)
        {
            entityKey = key;
            guildName = name;
        }
    }
    [SerializeField]
    GameObject guildCreatePanel;

    [SerializeField]
    GameObject guild_content, guild_content_prefab;

    [SerializeField]
    GameObject guild_info_panel, guild_info_content, guild_member_prefab;

    [SerializeField]
    GameObject guild_personal_panel, guild_personal_content, guild_personal_prefab;

    [SerializeField]
    NotificationManager notificationManager;

    List<PlayFab.ClientModels.GetUserDataResult> resultList = new();
    List<PlayFab.AdminModels.PlayerProfile> listOfPlayers;
    List<GuildInfo> guildInfos = new();
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
            listOfPlayers = result.PlayerProfiles;
            resultList.Clear();
            foreach (PlayFab.AdminModels.PlayerProfile profile in listOfPlayers)
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
                //Debug.Log(r.Data["entityID"].Value);
                //Debug.Log(r.Data["entityType"].Value);

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

        guildInfos.Clear();

        foreach (var pair in response.Groups)
        {
            Debug.Log("Guild Found");

            GroupNameById[pair.Group.Id] = pair.GroupName;
            
            EntityGroupPairs.Add(new KeyValuePair<string, string>(prevRequest.Entity.Id, pair.Group.Id));

            GuildInfo newInfo = new(pair.Group, pair.GroupName); 
            guildInfos.Add(newInfo);

        }
    }

    public IEnumerator ListGuilds()
    {
        yield return new WaitForSeconds(1f); 

        foreach (Transform child in guild_content.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var pair in guildInfos)
        {
            GameObject newGuildContent = Instantiate(guild_content_prefab, guild_content.transform);

            // Assuming there is a Text component in your prefab, set its text to the group name
            newGuildContent.GetComponent<Button>().onClick.AddListener(() => 
            { 
                OpenGuildInfo(pair.guildName, pair.entityKey); 
            });

            TMP_Text textComponent = newGuildContent.transform.Find("GuildNameText").GetComponent<TMP_Text>();
            textComponent.text = pair.guildName;
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
        PlayFabClientAPI.UpdateUserData(req, result => 
        { 
            Debug.Log("Updated Guild Leader"); 
        }
        , error => { Debug.Log("Failed to update guild leader"); });

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

    public void ApplyToGroup(string groupId)
    {
        // A player-controlled entity applies to join an existing group (of which they are not already a member)
        var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId) };
        PlayFabGroupsAPI.ApplyToGroup(request, r=> { Debug.Log("Successfully applied for group."); }, OnSharedError);
    }
    public void AcceptApplication(EntityKey group, EntityKey playerEntity)
    {
        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupApplicationRequest { Group = group, Entity = playerEntity };
        PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
    }
    public void OnAcceptApplication(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupApplicationRequest)response.Request;
        notificationManager.notification_count--;
        notificationManager.Update_NotificationCount();
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
    public void CloseGuildInfo()
    {
        //guildCreatePanel.SetActive(false);
        guild_info_panel.SetActive(false);
    }

    public void OpenGuildInfo(string groupName, EntityKey group)
    {
        foreach(Transform child in guild_info_content.transform)
        {
            Destroy(child);
        }

        int memberCount = 0;
        var req = new ListGroupMembersRequest() { Group = group };
        PlayFabGroupsAPI.ListGroupMembers(req, 
            result => {
               
                foreach (var entities in result.Members)
                {
                    var temp = entities.Members;

                    foreach (var individualMember in temp)
                    {
                        GameObject memberLabel = Instantiate(guild_member_prefab, guild_info_content.transform);
                        memberLabel.GetComponent<TMP_Text>().text = individualMember.Key.Id;
                        //Debug.Log(individualMember.Key.Id);
                        memberCount++;
                    }
                }
                guild_info_panel.transform.Find("Guild_NoOfMembers").GetComponent<TMP_Text>().text = "Members: " + memberCount;

            }, 
            e => {
                Debug.Log(e.GenerateErrorReport());
            });

        guild_info_panel.transform.Find("GuildName_Label").GetComponent<TMP_Text>().text = groupName;
        guild_info_panel.transform.Find("Button_GuildApply").GetComponent<Button>().onClick.AddListener(() => { ApplyToGroup(group.Id); });
        guild_info_panel.SetActive(true);
    }

    public void OpenPersonalGuildInfo()
    {
        int memberCount = 0;
        var request = new ListMembershipRequest();
        PlayFabGroupsAPI.ListMembership(request,
            r => {

                foreach (var group in r.Groups)
                {
                    guild_personal_panel.transform.Find("GuildName_Label").GetComponent<TMP_Text>().text = group.GroupName;

                    var req = new ListGroupMembersRequest() { Group = group.Group };
                    PlayFabGroupsAPI.ListGroupMembers(req, result =>
                    {
                        foreach (var entities in result.Members)
                        {
                            var temp = entities.Members;

                            foreach (var individual in temp)
                            {
                                GameObject memberLabel = Instantiate(guild_personal_prefab, guild_personal_content.transform);
                                memberLabel.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = individual.Key.Id;
                                memberCount++;
                                memberLabel.transform.Find("RankText").GetComponent<TMP_Text>().text = memberCount.ToString();
                            }
                        }
                        guild_personal_panel.transform.Find("MembersCount").GetComponent<TMP_Text>().text = "Members: " + memberCount;

                    }, error => { Debug.Log(error); });
                }
            },
            e => { Debug.Log(e); });

        guild_personal_panel.SetActive(true);
    }
    public void ClosePersonalGuildInfo()
    {
        foreach (Transform child in guild_personal_content.transform)
        {
            Destroy(child.gameObject);
        }

        guild_personal_panel.SetActive(false);
    }

    
}
