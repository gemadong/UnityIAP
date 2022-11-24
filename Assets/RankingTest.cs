using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayNANOO;

public class RankingTest : MonoBehaviour
{
    Plugin plugin;

    void Awake()
    {
        plugin = Plugin.GetInstance();
    }
    //랭킹
    public void RankingRecordButton()
    {
        plugin.RankingRecord("dbtest-RANK-F4644B11-9C3EC26A", 0, "{PLAYER_DATA}", (state, message, rawData, dictionary) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    public void RankingPersonalButton()
    {
        plugin.RankingPersonal("dbtest-RANK-F4644B11-9C3EC26A", (state, message, rawData, dictionary) =>
        {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("ranking" + dictionary["ranking"]);
                Debug.Log("data" + dictionary["data"]);
                Debug.Log("total_player" + dictionary["total_player"]);
                //playerData = dictionary["data"].ToString();
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }
    //데이터 저장
    public void StorageSave()
    {
        plugin.Storage.Save("Gema", "Dong", true, (state, error, jsonString, values) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    public void StorageLoad()
    {
        plugin.Storage.Load("Gema", (state, message, rawData, dictionary) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("StorageKey"+dictionary["StorageKey"]);
                Debug.Log("StorageValue"+dictionary["StorageValue"]);
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }
    //우편함
    public void InboxManagerCount()
    {
        plugin.InboxManager.Count("dbtest-inbox-9A43EFF8", (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Count : " + values["Count"]);
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        plugin.InboxManager.Items("dbtest-inbox-9A43EFF8", (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                foreach (Dictionary<string, object> value in (ArrayList)values["Items"])
                {
                    Debug.Log("Type : "+value["Type"]);
                    Debug.Log("ItemKey : "+value["ItemKey"]);
                    Debug.Log("ExpireSec : "+value["ExpireSec"]);

                    PlayNANOO.Inbox.ItemValueModel[] items = value["Items"] as PlayNANOO.Inbox.ItemValueModel[];
                    foreach (PlayNANOO.Inbox.ItemValueModel item in items)
                    {
                        Debug.Log("item.item_code : "+item.item_code);
                        Debug.Log("item.item_count : " +item.item_count);
                    }

                    PlayNANOO.Inbox.MessageValueModel[] messages = value["Messages"] as PlayNANOO.Inbox.MessageValueModel[];
                    foreach (PlayNANOO.Inbox.MessageValueModel message in messages)
                    {
                        Debug.Log("message.language : "+message.language);
                        Debug.Log("message.title : "+message.title);
                        Debug.Log("message.content : "+message.content);
                    }
                }
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        plugin.InboxManager.Clear("dbtest-inbox-9A43EFF8", (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("InboxManager.Clear Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    //쿠폰
    public void CouponButton()
    {
        plugin.Coupon("TESTEVENT-GTLYXI5J", (state, message, rawData, dictionary) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("code : "+dictionary["code"]);
                Debug.Log("item_code : "+dictionary["item_code"]);
                Debug.Log("item_count : "+dictionary["item_count"]);
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    //캐시 데이터
    public void CacheSetButton()
    {
        plugin.CacheSet("TestCache1", "10", 604800, (state, message, rawData, dictionary) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Success");
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        plugin.CacheGet("TestCache", (state, message, rawData, dictionary) => {
            if (state.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("TestCache value : "+dictionary["value"].ToString());
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    //게임재화
    public void Currenct()
    {
        plugin.CurrencyAll((status, errorMessage, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                foreach (Dictionary<string, object> item in (ArrayList)values["items"])
                {
                    Debug.Log("currency : "+item["currency"]);
                    Debug.Log("amount : "+item["amount"]);
                }
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        plugin.CurrencyCharge("TD", 10000, (status, errorMessage, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("amount : "+values["amount"]);
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }

    //길드
    public void GuildButton()
    {
        plugin.Guild.Search("dbtest-guild-D9C64451", PlayNANOO.Guild.SortCondition.RANDOM, PlayNANOO.Guild.SortType.DESC, 1, (status, error, jsonString, values) =>
        {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                foreach (Dictionary<string, object> value in (ArrayList)values["Items"])
                {
                    Debug.Log(value["TableCode"]);
                    Debug.Log("Uid : "+value["Uid"]);
                    Debug.Log(value["Name"]);
                    Debug.Log(value["Point"]);
                    Debug.Log(value["MasterUuid"]);
                    Debug.Log(value["MasterNickname"]);
                    Debug.Log(value["Country"]);
                    Debug.Log(value["MemberCount"]);
                    Debug.Log(value["MemberLimit"]);
                    Debug.Log(value["AutoAuth"]);
                    Debug.Log(value["InDate"]);
                }
            }
            else
            {
                Debug.Log("Fail");
            }
        });
        plugin.Guild.PersonalWithdraw("dbtest-guild-D9C64451", (status, errorCode, jsonString, values) => {
            if (status.Equals(Configure.PN_API_STATE_SUCCESS))
            {
                Debug.Log("Status : "+values["Status"].ToString());
            }
            else
            {
                Debug.Log("Fail");
            }
        });
    }
}
