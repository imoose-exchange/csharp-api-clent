using System;
using System.Collections.Generic;
using System.Text;

namespace ImooseApiClient.Models.Response
{
    class ImooseResponse<T>
    {
        public string[] errors { get; set; }
    
        public T data { get; set; }

        public string code { get; set; }

        public string getErrorMessage()
        {
            if(errors != null)
            {
                return string.Join(" : ", errors);
            }

            return string.Empty;
        }

    }
}
