using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    public class DataIO
    {
        WebClient wc = new WebClient();
        string rpcUrl;
        public DataIO(string rpcurl)
        {
            this.rpcUrl = rpcurl;
        }

        /// <summary>
        /// 取得当前区块高度
        /// </summary>
        /// <returns></returns>
        public async Task<int> rpc_getBlockCount()
        {
            var gstr = this.makeRpcUrlGet(this.rpcUrl,NeoMethod.rpc_getblockcount);
            try
            {
                var json = await this.downLoadString(gstr);
                if (json["result"] != null)
                {
                    int height = json["result"].AsInt();
                    return height;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("Fail to get block count. **************" + err.ToString());
                return -1;
            }
        }


        /// <summary>
        /// 加载指定区块的数据
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <returns></returns>
        public async Task<MyJson.JsonNode_Object> rpc_loadBlockData(int blockIndex)
        {
            var gstr = makeRpcUrlGet(this.rpcUrl,NeoMethod.rpc_getblock, blockIndex.ToString(), "1");
            try
            {
                var json = await this.downLoadString(gstr);
                var result = json["result"] as MyJson.JsonNode_Object;
                //var txs = json["result"].AsDict()["tx"].AsList();
                return result;
            }
            catch (Exception err)
            {
                Console.WriteLine("failed to load block data.Info:" + err.ToString());
                return null;
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<MyJson.JsonNode_Object> downLoadString(string url)
        {
            var str = await wc.DownloadStringTaskAsync(url);
            var json = MyJson.Parse(str).AsDict();
            bool beError = json.ContainsKey("error");
            if (beError)
            {
                return null;
            }
            else
            {
                return json;
            }
        }

        public MyJson.JsonNode_Object downLoadString_newclient(string url)
        {
            var wc = new WebClient();
            var str = wc.DownloadString(url);
            var json = MyJson.Parse(str).AsDict();
            bool beError = json.ContainsKey("error");
            if (beError)
            {
                return null;
            }
            else
            {
                return json;
            }
        }

        public async Task<string> PostData(string data)
        {
            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add("content-type", "text/plain;charset=UTF-8");
            byte[] postdata = System.Text.Encoding.UTF8.GetBytes(data);

            byte[] retdata = await wc.UploadDataTaskAsync(this.rpcUrl, "POST", postdata);
            return Encoding.UTF8.GetString(retdata);
        }

        /// <summary>
        /// 拼装 url  get
        /// </summary>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string makeRpcUrlGet(string url, string method, params string[] param)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(url + "?jsonrpc=2.0&id=1&method=" + method + "&params=[");
            for (int i = 0; i < param.Length; i++)
            {
                if (i != param.Length - 1)
                {
                    sb.Append(param[i] + ",");
                }
                else
                {
                    sb.Append(param[i]);
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
        /// <summary>
        /// 拼装 url  psot
        /// </summary>
        /// <param name="method"></param>
        /// <param name="_params"></param>
        /// <returns></returns>
        public string MakeRpcUrlPost(string method, params string[] _params)
        {
            var json = new MyJson.JsonNode_Object();
            json.SetDictValue("id", 1);
            json.SetDictValue("jsonrpc", "2.0");
            json.SetDictValue("method", method);

            var array = new MyJson.JsonNode_Array();
            for (var i = 0; i < _params.Length; i++)
            {
                array.Add(new MyJson.JsonNode_ValueString(_params[i]));
            }
            json.SetDictValue("params", array);
            string urldata = json.ToString();
            return urldata;
        }


        public async Task<string> rpc_Transaction(string url, string rawdata)
        {
            string urldata = this.MakeRpcUrlPost(NeoMethod.rpc_sendrawtransaction, rawdata);
            byte[] postdata = System.Text.Encoding.UTF8.GetBytes(urldata);

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add("content-type", "text/plain;charset=UTF-8");
            try
            {
                byte[] retdata = await wc.UploadDataTaskAsync(url, "POST", postdata);
                return System.Text.Encoding.UTF8.GetString(retdata);
            }
            catch (Exception e)
            {
                Console.WriteLine("post transaction:" + e.Message);
                return null;
            }
        }

        public string rpc_Transaction_3(string url, string rawdata)
        {
            string urldata = this.MakeRpcUrlPost(NeoMethod.rpc_sendrawtransaction, rawdata);
            byte[] postdata = System.Text.Encoding.UTF8.GetBytes(urldata);

            WebClient wc = new WebClient();
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add("content-type", "text/plain;charset=UTF-8");
            try
            {
                byte[] retdata = wc.UploadDataTaskAsync(url, "POST", postdata).Result;
                return System.Text.Encoding.UTF8.GetString(retdata);
            }
            catch (Exception e)
            {
                Console.WriteLine("post transaction:" + e.Message);
                return null;
            }
        }

        public string rpc_Transaction_2(string url, string rawdata)
        {
            string urldata = this.MakeRpcUrlPost(NeoMethod.rpc_sendrawtransaction, rawdata);
            HttpClient wc = new HttpClient();
            HttpResponseMessage httpResponseMessage = wc.PostAsync(url, new StringContent(urldata)).Result;
            string json = httpResponseMessage.Content.ReadAsStringAsync().Result;

            return json;
        }

        public string rpc_InvocationTransaction(string url, string rawdata)
        {
            string urldata = this.MakeRpcUrlPost(NeoMethod.rpc_sendrawtransaction, rawdata);
            HttpClient wc = new HttpClient();
            HttpResponseMessage httpResponseMessage = wc.PostAsync(url, new StringContent(urldata)).Result;
            string json = httpResponseMessage.Content.ReadAsStringAsync().Result;

            return json;
        }

        public  string rpc_getstorage(string url,string scriptaddress, string key)
        {
            var urldata = this.makeRpcUrlGet(url,NeoMethod.rpc_getstorage, scriptaddress, key);
            //var data = await this.downLoadString_newclient(urldata);
            var res = this.downLoadString_newclient(urldata);
            return res.ToString();
        }

        public string rpc_getstorage_2(string url, string scriptaddress, string key)
        {
            var urldata = this.MakeRpcUrlPost(NeoMethod.rpc_getstorage, scriptaddress, key);
            //var data = await this.downLoadString_newclient(urldata);
            var res = this.httpPostData(url, urldata);
            return res.ToString();
        }

        public string rpc_invokescript(string url,string script)
        {
            var urldata = this.MakeRpcUrlPost(NeoMethod.rpc_invokescript, script);
            //var res = await this.PostData(urldata);
            var res = this.httpPostData(url,urldata);
            return res;
        }


        public string httpPostData(string url,string data)
        {
            HttpClient wc = new HttpClient();
            HttpResponseMessage httpResponseMessage =wc.PostAsync(url, new StringContent(data)).Result;
            string json = httpResponseMessage.Content.ReadAsStringAsync().Result;
            return json;
        }

        //public async Task<string> rpc_getapplicationlog(string url)
        //{
        //    string urldata = this.MakeRpcUrlPost(NeoMethod.rpc_sendrawtransaction, rawdata);
        //    byte[] postdata = System.Text.Encoding.UTF8.GetBytes(urldata);

        //    WebClient wc = new WebClient();
        //    wc.Encoding = Encoding.UTF8;
        //    wc.Headers.Add("content-type", "text/plain;charset=UTF-8");
        //    try
        //    {
        //        byte[] retdata = await wc.UploadDataTaskAsync(url, "POST", postdata);
        //        return System.Text.Encoding.UTF8.GetString(retdata);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("post transaction:" + e.Message);
        //        return null;
        //    }
        //}

    }
}
