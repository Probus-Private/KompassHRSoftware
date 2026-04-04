using System;
using System.Net.Http;
using System.Text;

namespace KompassHR.Areas.ESS.Controllers.ESS_MyPolicyAndLibrary
{
    internal class StringContent
    {
        private string jsonBody;
        private Encoding uTF8;
        private string v;

        public StringContent(string jsonBody, Encoding uTF8, string v)
        {
            this.jsonBody = jsonBody;
            this.uTF8 = uTF8;
            this.v = v;
        }

        public static implicit operator HttpContent(StringContent v)
        {
            throw new NotImplementedException();
        }
    }
}