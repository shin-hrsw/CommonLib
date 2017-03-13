using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Base
{
    public abstract class EditorBase<T> where T : ViewModelBase, new()
    {
        #region プロパティ
        public List<T> ViewModelList { get; set; }

        public string ErrorMessage { get; protected set; }
        #endregion

        #region メソッド(public)
        public virtual bool Update()
        {
            if (this.ViewModelList.Count(x => x.HasError) > 0)
            {
                var v = this.ViewModelList.Select((x, index) => new { Index = index + 1, Data = x })
                    .First(y => y.Data.HasError);
                this.ErrorMessage = v.Data.SearchError() + "\n(行番号：" + v.Index + ")";
                return false;
            }

            foreach(var v in ViewModelList)
            {
                if (!v.Update())
                {
                    this.ErrorMessage = v.ErrorMessage;
                    return false;
                }
            }
            return true;
        }

        public virtual T AddNew() => new T();

        public virtual void Delete(int idx)
        {
            if(idx < 0 || idx >= ViewModelList.Count) { return; }
            ViewModelList[idx].Delete();
        }
        #endregion
    }
}
