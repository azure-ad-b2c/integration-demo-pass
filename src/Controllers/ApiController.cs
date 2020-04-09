using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace B2C.PASSDemo.Controllers
{
    [ApiController]
    [RequireHttps]
    public class ApiController : Controller
    {
        [HttpPost]
        [Route("decryptclaims")]
        [AllowAnonymous]
        public async Task<IActionResult> DecryptClaims([FromBody] Dictionary<string, string> content)
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return BadRequest("Authorization header is required.");
            }

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            var secret = authHeader.Parameter.Replace("Bearer ", "");

            if (secret.Length < 16)
            {
                return BadRequest("Secret value provided is of insufficient length.");
            }

            var secretBytes = Encoding.UTF8.GetBytes(secret.Substring(0, 16));

            var outputClaims = new Dictionary<string, string>();

            using (var aesAlg = Aes.Create())
            {
                var decryptor = aesAlg.CreateDecryptor(secretBytes, secretBytes);

                foreach (var kv in content)
                {
                    var cipherText = Convert.FromBase64String(content[kv.Key]);

                    using (var cipherStream = new MemoryStream(cipherText))
                    {
                        using (var cryptoStream = new CryptoStream(cipherStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (var textReader = new StreamReader(cryptoStream))
                            {
                                outputClaims[kv.Key] = await textReader.ReadToEndAsync();
                            }
                        }
                    }
                }
            }

            return new JsonResult(outputClaims);
        }
    }
}
