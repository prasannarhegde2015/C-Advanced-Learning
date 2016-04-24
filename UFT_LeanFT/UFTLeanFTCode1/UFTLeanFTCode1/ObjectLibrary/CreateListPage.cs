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
    class CreateListPage
    {

        private IBrowser _brcreatelistpage;
        public IBrowser brcreatelistpage
        {
            set
            {
                this._brcreatelistpage = value;
            }
            get
            {
                return this._brcreatelistpage;
            }
        }


        public IEditField tbfistname
        {
            get
            {

                return brcreatelistpage.Describe<IEditField>(new EditFieldDescription { Name = "firstname" });
            }

        }
        public IEditField tblastname
        {
            get
            {

                return brcreatelistpage.Describe<IEditField>(new EditFieldDescription { Name = "lastname" });
            }

        }
        public IEditField tbemail
        {
            get
            {

                return brcreatelistpage.Describe<IEditField>(new EditFieldDescription { Name = "email" });
            }

        }
        public IEditField tbphone
        {
            get
            {

                return brcreatelistpage.Describe<IEditField>(new EditFieldDescription { Name = "phone" });
            }

        }
        public IEditField tbaddress
        {
            get
            {

                return brcreatelistpage.Describe<IEditField>(new EditFieldDescription { Name = "address" });
            }

        }

        public IButton btnsubmit
        {
            get
            {

                return brcreatelistpage.Describe<IButton>(new ButtonDescription { Name = "SAVE" });
            }

        }
    }
}
