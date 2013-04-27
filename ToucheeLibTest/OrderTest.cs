using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;
using System.Collections.Generic;
using Touchee;
using Touchee.Playback;

namespace ToucheeLibTest {
    
    [TestClass]
    public class OrderTest {

        [TestMethod]
        public void OrderTest2() {

            var list = new List<string> { "vbla", "ebla", "{bla}" };
            var sorted = list.OrderBy(t => t);


        }

    }
}
