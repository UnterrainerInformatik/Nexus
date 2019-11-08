// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using NexusServer;
using NexusServer.DTOs;
using NexusServer.JSONs;
using NUnit.Framework;

namespace NexusImplementation
{
    [TestFixture]
    [System.ComponentModel.Category("REST")]
    public class ServerTests
    {
        public const string URL_CONNECTION = "http://localhost:58500/Connection.svc";

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        [SetUp]
        public void Init()
        {
        }

        /// <summary>
        ///     Finishes this instance.
        /// </summary>
        [TearDown]
        public void Finish()
        {
        }

        [Test]
        [System.ComponentModel.Category("REST.Simple")]
        public void TestMessageString()
        {
            var p = new WebClient();
            var url = URL_CONNECTION + "/ping";
            var data = p.DownloadData(url);
            Stream stream = new MemoryStream(data);
            var streamReader = new StreamReader(stream);
            var response = streamReader.ReadToEnd();
            Assert.AreEqual("{\"PingResult\":\"Pong\"}", response);
        }

        [Test]
        [System.ComponentModel.Category("REST.Simple")]
        public void TestMessageJsonReader()
        {
            var p = new WebClient();
            var url = URL_CONNECTION + "/ping";
            var data = p.DownloadData(url);
            Stream stream = new MemoryStream(data);
            XmlReader r = JsonReaderWriterFactory.CreateJsonReader(stream, new XmlDictionaryReaderQuotas());

            var root = XElement.Load(r);
            var result = root.XPathSelectElement("//PingResult");
            Assert.AreEqual("Pong", result.Value);
        }

        [Test]
        [System.ComponentModel.Category("REST.Simple")]
        public void TestMessageJsonObject()
        {
            var p = new WebClient();
            var url = URL_CONNECTION + "/ping";
            var data = p.DownloadData(url);
            Stream stream = new MemoryStream(data);
            var streamReader = new StreamReader(stream);
            var response = streamReader.ReadToEnd();

            var r = JsonConvert.DeserializeObject<PingJson>(response);
            var d = Static.Mapper.Map<PingDto>(r);
            Assert.AreEqual("Pong", d.PingResult);
        }
    }
}