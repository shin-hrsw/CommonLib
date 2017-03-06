using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib.Base
{
    /// <summary>
    /// Modelの基底クラス
    /// </summary>
    /// <remarks>
    /// データベースのテーブルと1対1で対応(テーブルひとつにつきModelひとつ)
    /// </remarks>
    abstract class ModelBase
    {
        #region プロパティ(abstract)
        /// <summary>
        /// テーブル名
        /// </summary>
        /// <remarks>対応するテーブルの名前を設定する</remarks>
        abstract protected string TableName { get; }

        /// <summary>
        /// 主キー情報
        /// </summary>
        /// <remarks>主キーの名前、データ型を設定する</remarks>
        abstract protected Dictionary<string,Type> KeyInfomation { get; } 

        /// <summary>
        /// AUTO INCREMENTを使用するかどうか
        /// </summary>
        /// <remarks>
        /// trueの場合はinsert時に主キーが列名からはずれる。
        /// また、insert後にlast_insert_id()の値をキーに設定する
        /// </remarks>
        abstract protected bool UseAutoIncrement { get; }
        #endregion

        #region メソッド(public)
        public bool Update()
        {
            return true;
        }
        #endregion
    }
}
