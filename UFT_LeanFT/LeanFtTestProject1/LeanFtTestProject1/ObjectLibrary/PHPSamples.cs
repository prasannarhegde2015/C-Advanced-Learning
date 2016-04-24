using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HP.LFT.Common;
using HP.LFT.SDK;
using HP.LFT.SDK.Configuration;
using HP.LFT.SDK.Web;
using HP.LFT.Common.Utils;

namespace LeanFtTestProject1.ObjectLibrary
{
    class PHPSamples
    {

        private IBrowser _brphpsampleslistpage;
        public IBrowser brphpsampleslistpage
        {
            set
            {
                this._brphpsampleslistpage = value;
            }
            get
            {
                return this._brphpsampleslistpage;
            }
        }


        public ILink lnkcreatelist
        {
            get
            {

                return brphpsampleslistpage.Describe<ILink>(new LinkDescription { InnerText = "Create Web List." });
            }

        }

    }
}
