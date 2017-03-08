using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Base
{
    /// <summary>
    /// ViewModelの基底クラス
    /// </summary>
    /// <remarks>
    /// IDataErrorInfoインターフェースとINotifyPropertyChangedインターフェースを実装する。
    /// DataBindingを行うクラスはこのクラスを継承する
    /// </remarks>
    public abstract class ViewModelBase : IDataErrorInfo, INotifyPropertyChanged
    {
        private Dictionary<string, string> error_info;

        #region コンストラクタ
        public ViewModelBase()
        {
            this.error_info = new Dictionary<string, string>();
        }
        #endregion

        #region IDataErrorInfoの実装
        public string this[string columnName]
        {
            get { return this.error_info.ContainsKey(columnName) ? this.error_info[columnName] : null; }
        }

        public string Error
        {
            get { return null; }
        }
        #endregion

        #region INotifyPropertyChangedの実装
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if(this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
