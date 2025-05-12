using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;

namespace CloudCode
{
    public class EchoString
    {
        [CloudCodeFunction("EchoString")]
        public async Task<string> Echo(string input)
        {
            return "Arg :" + input;
        }
    }
}
