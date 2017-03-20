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
    /// DataBindingを行うクラスはこのクラスを継承する。
    /// また、Modelのデータ整合性はViewModelでチェックすること。
    /// </remarks>
    public abstract class ViewModelBase : IDataErrorInfo, INotifyPropertyChanged
    {
        protected Dictionary<string, string> error_info;

        #region プロパティ
        /// <summary>
        /// ステータス
        /// </summary>
        public RowStatus Status => this.Model.Status;

        /// <summary>
        /// エラーメッセージ
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// エラー有無
        /// </summary>
        public bool HasError
        {
            get
            {
                if (this.error_info.Count > 0) { return true; }
                if (!string.IsNullOrWhiteSpace(this.Error)) { return true; }

                return false;
            }
        }
        #endregion

        #region コンストラクタ
        public ViewModelBase()
        {
            this.error_info = new Dictionary<string, string>();
        }
        #endregion

        #region メソッド(public)
        public virtual bool Update()
        {
            this.ErrorMessage = string.Empty;

            // 入力不備があれば処理しない
            if (this.HasError)
            {
                this.ErrorMessage = SearchError();
                return false;
            }

            if (!this.Model.Update())
            {
                this.ErrorMessage = this.Model.GetError();
                return false;
            }

            return true;
        }

        public virtual void Delete()
        {
            this.Model.Delete();
        }

        public string SearchError()
        {
            // 個別のプロパティについて入力不備があればそれを返す
            if (this.error_info.Count > 0)
            {
                return this.error_info.First().Value;
            }

            // その他に入力不備があれば返す
            if (string.IsNullOrWhiteSpace(this.Error)) { return this.Error; }

            return null;
        }
        #endregion

        #region abstractプロパティ
        /// <summary>
        /// 更新対象のModel
        /// </summary>
        /// <remarks>設定忘れを防ぐために抽象プロパティにする</remarks>
        protected abstract ModelBase Model { get; }
        #endregion

        #region IDataErrorInfoの実装
        public string this[string columnName]
        {
            get { return this.error_info.ContainsKey(columnName) ? this.error_info[columnName] : null; }
        }

        public virtual string Error
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
