using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NELLearn
{
    struct BlockJson
    {
        string jsonrpc;
        string id;
        Blockresult result;
    }
    struct Blockresult
    {
        string hash;
        int size;
        int version;
        string previousblockhash;
        string nextblockhash;
        string merkleroot;
        int time;
        int index;
        string nonce;
        string nextconsensus;
        Blockscript script;
        int confirmations;
        TransationStruct tx;
    }

    struct Blockscript
    {
        string invocation;
        string verification;
    }

    struct TransationStruct
    {
        string txid;
        int size;
        string type;
        int version;
        
        TransationVin[] vin;
        TransationVin[] vout;
        int sys_fee;
        int net_fee;

        int nonce;//大

        //not sure array type
        Array attributes;
        Array scripts;
    }

    struct TransationVin
    {
        string txid;
        int vout;
    }

    struct TransationVout
    {
        int n;
        string asset;//什么资产
        string value;//给多少
        string address;//给谁
    }


}
