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

namespace UFTLeanFTCode1.ObjectLibrary
{
    class HomePage
        {
        private IBrowser _brhomepage;
        public IBrowser brhomepage 
        {
            set 
            {
                this._brhomepage = value;
            }
            get{
                return this._brhomepage;
            }
        }

    
        public ILink lnkphpsamples
        {
           get {  
               
                   return brhomepage.Describe<ILink>(new LinkDescription { InnerText = "PHP Samples" });
               }
           
        }

        public ILink lnkhomeforum
        {
            get
            {

                return brhomepage.Describe<ILink>(new LinkDescription { InnerText = "Home Forum" });
            }

        }
    }
}
