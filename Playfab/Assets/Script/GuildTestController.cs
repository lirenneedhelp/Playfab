using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.GroupsModels;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using System;

public class GuildTestController : MonoBehaviourPun
{
    class EntityInfo
    {
        public string displayName;
    }

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
    GameObject guild_member_panel;

    [SerializeField]
    NotificationManager notificationManager;

    List<PlayFab.ClientModels.GetUserDataResult> resultList = new();
    List<PlayFab.AdminModels.PlayerProfile> listOfPlayers;
    List<GuildInfo> guildInfos = new();

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
                var request = new ListMembershipRequest
                {
                    Entity = new EntityKey { Id = r.Data["entityID"].Value, Type = r.Data["entityType"].Value }
                };
                PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
            }
        }

    }
    private void OnListGroups(ListMembershipResponse response)
    {
        var prevRequest = (ListMembershipRequest)response.Request;
        Debug.Log("Finding Guild List");

        foreach (var pair in response.Groups)
        {
            Debug.Log("Guild Found");

            GuildInfo newInfo = new(pair.Group, pair.GroupName); 

            if (!guildInfos.Contains(newInfo))
                guildInfos.Add(newInfo);

        }

        ListGuilds();
    }

    public void ListGuilds()
    {
        //Removes Guild on the list
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
        DataCarrier.Instance.guildStats.currentLevel++;
        DataCarrier.Instance.guildStats.discountBonus++;
        GuildLevelSystem.Instance.CalculateNextLevelXp();


        // Create the policy statements as per your requirements
        List<PlayFab.ProfilesModels.EntityPermissionStatement> statements = new List<PlayFab.ProfilesModels.EntityPermissionStatement>
        {
            // Policy 1
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "Read",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*",
                Principal = "*",
                Comment = "Allow all entities to read the group's metadata, such as name"
            },

            // Policy 2
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "*",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*",
                Principal = new Dictionary<string, object>
                {
                    { "MemberOf", new Dictionary<string, object> { { "RoleId", "admins" } } }
                },
                Comment = "Allow members of the group administrator role to modify the group metadata"
            },

            // Policy 3
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "Read",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:data--*!*/Profile/*",
                Principal = "*",
                Comment = "Allow members of the group to read entity profile data and files"
            },

            // Policy 4
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "*",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:data--*!*/*",
                Principal = new Dictionary<string, object>
                {
                    { "MemberOf", new Dictionary<string, object> { { "RoleId", "admins" } } }
                },
                Comment = "Allow members of the group administrator role to modify group profile data and files"
            },

            // Policy 5
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "*",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/*",
                Principal = new Dictionary<string, object>
                {
                    { "MemberOf", new Dictionary<string, object> { { "RoleId", "admins" } } }
                },
                Comment = "Allow members of the group administrator role to do anything with the group"
            },

            // Policy 6
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "Read",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/Members/*",
                Principal = new Dictionary<string, object>
                {
                    { "ChildOf", new Dictionary<string, object> { { "EntityType", "title" }, { "EntityId", "6789D" } } }
                },
                Comment = "Allow members of the group to view members of the group"
            },

            // Policy 7
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "Read",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/Roles/*",
                Principal = new Dictionary<string, object>
                {
                    { "ChildOf", new Dictionary<string, object> { { "EntityType", "title" }, { "EntityId", "6789D" } } }
                },
                Comment = "Allow all entities of the title to read all roles in the group"
            },

            // Policy 8
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "Create",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/Applications/*",
                Principal = new Dictionary<string, object>
                {
                    { "ChildOf", new Dictionary<string, object> { { "EntityType", "title" }, { "EntityId", "6789D" } } }
                },
                Comment = "Allow all entities to apply to join the group"
            },

            // Policy 9
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "RemoveMember",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/Members/[SELF]",
                Principal = new Dictionary<string, object>
                {
                    { "ChildOf", new Dictionary<string, object> { { "EntityType", "title" }, { "EntityId", "6789D" } } }
                },
                Comment = "Allow entities to leave the group"
            },

            // Policy 10
            new PlayFab.ProfilesModels.EntityPermissionStatement
            {
                Action = "RemoveMember",
                Effect = PlayFab.ProfilesModels.EffectType.Allow,
                Resource = "pfrn:group--*!*/Roles/*/Members/[SELF]",
                Principal = new Dictionary<string, object>
                {
                    { "ChildOf", new Dictionary<string, object> { { "EntityType", "title" }, { "EntityId", "6789D" } } }
                },
                Comment = "Allow entities to leave any role that they are in"
            }
        };

   
        var setPolicy = new PlayFab.ProfilesModels.SetEntityProfilePolicyRequest() { Entity = new() { Id = response.Group.Id, Type = response.Group.Type }, Statements = statements };
        PlayFabProfilesAPI.SetProfilePolicy(setPolicy, r => {
            Debug.Log("Policy Set");
            UpdateGroup(response.Group);
        }, e => { Debug.Log(e); });

        var req = new PlayFab.ClientModels.UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string> {
                { "entityID", DataCarrier.Instance.entityID},
                { "entityType", DataCarrier.Instance.entityType }
            },
            Permission = PlayFab.ClientModels.UserDataPermission.Public
        };
        PlayFabClientAPI.UpdateUserData(req, result => 
        { 
            Debug.Log("Updated Guild Leader");
            guildCreatePanel.SetActive(false);
        }
        , error => { Debug.Log("Failed to update guild leader"); });

        var prevRequest = (CreateGroupRequest)response.Request;
    }

    public void CloseCreatePanel()
    {
        guildCreatePanel.SetActive(false);
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
    public void DeclineApplication(EntityKey group, EntityKey playerEntity)
    {
        var req = new RemoveGroupApplicationRequest() { Group = group, Entity = playerEntity };
        PlayFabGroupsAPI.RemoveGroupApplication(req, result => { Debug.Log("Declined Application"); }, e => { Debug.Log(e); });
    }
    public void KickMember(string groupId, EntityKey entityKey)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
    }
    public void KickMember(string groupId, EntityKey entityKey, string role)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, result=> { 
            if (role == "Administrators")
            {
                var destroyReq = new DeleteGroupRequest() { Group = EntityKeyMaker(groupId) };
                PlayFabGroupsAPI.DeleteGroup(destroyReq, r => { Debug.Log("Destroyed Group Successfully"); }, e => { Debug.Log(e); });
            }
        }, OnSharedError);
    }
    private void OnKickMembers(EmptyResponse response)
    {
        var prevRequest= (RemoveMembersRequest)response.Request;
           
        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
    }



    public void OpenGuildCreatePanel()
    {
        guildCreatePanel.SetActive(true);
    }
    public void CloseGuildInfo()
    {
        //guildCreatePanel.SetActive(false);
        guild_info_panel.SetActive(false);
        guildInfos.Clear();
        resultList.Clear();
    }

    public void OpenGuildInfo(string groupName, EntityKey group)
    {
        foreach (Transform child in guild_info_content.transform)
        {
            Destroy(child.gameObject);
        }

        guild_info_panel.transform.Find("Button_GuildApply").gameObject.SetActive(true);

        int memberCount = 0;
        var req = new ListGroupMembersRequest() { Group = group };
        PlayFabGroupsAPI.ListGroupMembers(req,
            result => {

                foreach (var entities in result.Members)
                {
                    var temp = entities.Members;

                    foreach (var individualMember in temp)
                    {
                        string memberId = individualMember.Key.Id;

                        // Fetch entity data for the member
                        var getObjectsRequest = new PlayFab.DataModels.GetObjectsRequest()
                        {
                            Entity = new PlayFab.DataModels.EntityKey
                            {
                                Id = memberId,
                                Type = "title_player_account"  // Assuming player entities are of type "title_player_account"
                            }
                        };

                        PlayFabDataAPI.GetObjects(getObjectsRequest,
                            getObjectResult =>
                            {
                                // Check if the display name is present in the response
                                if (getObjectResult.Objects.TryGetValue("entity_info", out var entityInfo))
                                {
                                    EntityInfo temp = JsonUtility.FromJson<EntityInfo>(entityInfo.DataObject.ToString());
                                    string displayName = temp.displayName;
                                    GameObject memberLabel = Instantiate(guild_member_prefab, guild_info_content.transform);
                                    memberLabel.GetComponent<TMP_Text>().text = displayName;
                                    memberCount++;

                                    if (displayName == DataCarrier.Instance.displayName || DataCarrier.Instance.inGuild)
                                    {
                                        guild_info_panel.transform.Find("Button_GuildApply").gameObject.SetActive(false);
                                    }
                                }
                                else
                                {
                                    Debug.Log("Display name not found for player: " + memberId);
                                }

                                // Update member count and UI after processing all members
                                guild_info_panel.transform.Find("Guild_NoOfMembers").GetComponent<TMP_Text>().text = "Members: " + memberCount;
                            },
                            getObjectError =>
                            {
                                Debug.Log(getObjectError.GenerateErrorReport());
                            });
                    }
                }


                guild_info_panel.SetActive(true);
            },
            e => {
                Debug.Log(e.GenerateErrorReport());
            });

        guild_info_panel.transform.Find("GuildName_Label").GetComponent<TMP_Text>().text = groupName;
        guild_info_panel.transform.Find("Button_GuildApply").GetComponent<Button>().onClick.AddListener(() => { ApplyToGroup(group.Id); });
       
    }


    public void OpenPersonalGuildInfo()
    {
        int memberCount = 0;
        var request = new ListMembershipRequest();
        bool isAdmin = false;

        PlayFabGroupsAPI.ListMembership(request,
            r => {

                foreach (var group in r.Groups)
                {
                    guild_personal_panel.transform.Find("GuildName_Label").GetComponent<TMP_Text>().text = group.GroupName;
                    guild_personal_panel.transform.Find("GuildLevel").GetComponent<TMP_Text>().text = "Level: " + DataCarrier.Instance.guildStats.currentLevel.ToString();
                    guild_personal_panel.transform.Find("GuildExp").GetComponent<TMP_Text>().text = "EXP: " + DataCarrier.Instance.guildStats.currentExp.ToString() + " / " + DataCarrier.Instance.guildStats.expToLevelUp;

                    UpdateGroup(group.Group);

                    var req = new ListGroupMembersRequest() { Group = group.Group };
                    PlayFabGroupsAPI.ListGroupMembers(req, result =>
                    {
                        
                        foreach (var entities in result.Members)
                        {
                            var temp = entities.Members;
                            string memberRole = entities.RoleName;
                            
                            foreach (EntityWithLineage individual in temp)
                            {

                                string memberId = individual.Key.Id;
                                string memberType = individual.Key.Type;

                                if (DataCarrier.Instance.entityID == memberId && memberRole == "Administrators")
                                    isAdmin = true;

                                // Fetch entity data for the member
                                var getObjectsRequest = new PlayFab.DataModels.GetObjectsRequest()
                                {
                                    Entity = new PlayFab.DataModels.EntityKey
                                    {
                                        Id = memberId,
                                        Type = memberType  
                                    }
                                };
                                
                                PlayFabDataAPI.GetObjects(getObjectsRequest,
                                getObjectResult =>
                                {
                                    if (getObjectResult.Objects.TryGetValue("entity_info", out var entityInfo))
                                    {
                                        EntityInfo temp = JsonUtility.FromJson<EntityInfo>(entityInfo.DataObject.ToString());
                                        string displayName = temp.displayName;

                                        GameObject memberLabel = Instantiate(guild_personal_prefab, guild_personal_content.transform);
                                        memberLabel.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = displayName;

                                        memberCount++;
                                        memberLabel.transform.Find("RankText").GetComponent<TMP_Text>().text = memberCount.ToString();

                                        if (displayName == DataCarrier.Instance.displayName)
                                        {
                                            guild_personal_panel.transform.Find("LeaveGuild_Button").gameObject.SetActive(true);
                                            guild_personal_panel.transform.Find("LeaveGuild_Button").GetComponent<Button>().onClick.RemoveAllListeners();
                                            guild_personal_panel.transform.Find("LeaveGuild_Button").GetComponent<Button>().onClick.AddListener(
                                                () => {
                                                    KickMember(group.Group.Id, new() { Id = memberId, Type = memberType });
                                                    CloseGuildMemberInfo();
                                                    ClosePersonalGuildInfo();
                                                });

                                        }

                                        memberLabel.GetComponent<Button>().onClick.AddListener(
                                            () =>
                                            {
                                                //Means you're an admin
                                                if (isAdmin && displayName != DataCarrier.Instance.displayName)
                                                {
                                                    guild_member_panel.transform.Find("KickMember").gameObject.SetActive(true);
                                                    guild_member_panel.transform.Find("KickMember").GetComponent<Button>().onClick.AddListener(
                                                        () => {
                                                            KickMember(group.Group.Id, new() { Id = memberId, Type = memberType });
                                                            CloseGuildMemberInfo();
                                                            ClosePersonalGuildInfo();
                                                        });
                                                }
                                                else
                                                {
                                                    guild_member_panel.transform.Find("KickMember").gameObject.SetActive(false);
                                                }

                                                guild_member_panel.transform.Find("MemberNameValue_Text").GetComponent<TMP_Text>().text = displayName;
                                                guild_member_panel.transform.Find("MemberRoleValue_Text").GetComponent<TMP_Text>().text = memberRole;
                                                guild_member_panel.SetActive(true);
                                            });
                                    }
                                    else
                                    {
                                        Debug.Log("Display name not found for player: " + memberId);
                                    }

                                    // Update member count and UI after processing all members
                                    guild_personal_panel.transform.Find("MembersCount").GetComponent<TMP_Text>().text = "Members: " + memberCount;

                                },
                                getObjectError =>
                                {
                                    Debug.Log(getObjectError.GenerateErrorReport());
                                });        
                            }
                        }

                    }, error => { Debug.Log(error); });
                }
            },
            e => {
                guild_member_panel.transform.Find("LeaveGuild_Button").gameObject.SetActive(false);
                Debug.Log(e); 
            });

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
    public void CloseGuildMemberInfo()
    {
        guild_member_panel.SetActive(false);
    }
    public static void UpdateGroup(EntityKey groupId)
    {
        PlayFab.DataModels.SetObjectsRequest setGuildStats = new PlayFab.DataModels.SetObjectsRequest()
        {
            Entity = new() { Id = groupId.Id, Type = groupId.Type },
            Objects = new List<PlayFab.DataModels.SetObject>
                {
                    new PlayFab.DataModels.SetObject
                    {
                        ObjectName = "guild_info", 
                        DataObject = DataCarrier.Instance.guildStats
                    }
                }
        };

        PlayFabDataAPI.SetObjects(setGuildStats, r => { Debug.Log("Updated guild stats"); }, e => { Debug.Log(e.GenerateErrorReport()); });
    }

    
}
