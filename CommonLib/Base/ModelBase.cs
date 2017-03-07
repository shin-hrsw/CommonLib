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
    public abstract class ModelBase
    {
        // 制御で使用するプロパティのリスト
        // "NotTableColumnAttribute"のような属性を作って制御してもいいが…
        private List<string> control_properties = new List<string>()
        { "TableName", "KeyInfomation", "UseAutoIncrement" };

        private bool is_new = true;         // 新規データの場合にtrue
        private bool is_changed = false;    // 変更された場合にtrue
        private bool is_deleted = false;    // 削除された場合にtrue
        private string error;       // エラーが発生した場合に設定

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
            if (this.is_new)
            {
                return Insert();
            }

            if (this.is_changed)
            {
                return true;
            }

            if (this.is_deleted)
            {
                return true;
            }

            // SQL発行の必要がない
            return true;
        }

        public string GetError()
        {
            return this.error;
        }
        #endregion

        #region メソッド(private)
        private bool Insert()
        {
            Type ty = this.GetType();
            // プロパティから列リストを作る
            // 制御用プロパティは除外
            // AUTO_INCREMENTを使用する場合主キーは除外
            var props = ty.GetProperties()
                .Where(x =>
                {
                    if (this.control_properties.Contains(x.Name)) { return false; }
                    if (this.UseAutoIncrement)
                    {
                        if (this.KeyInfomation.ContainsKey(x.Name)) { return false; }
                        return true;
                    }
                    else { return true; }
                });
            string columns = "";
            string par = "";
            var param_list = new List<Database.SQLParameter>();
            foreach (var p in props)
            {
                columns += p.Name + ", ";
                par += "@" + p.Name + ", ";
                param_list.Add(new Database.SQLParameter(
                    "@" + p.Name, p.GetType(), p.GetValue(this)));
            }
            columns = columns.Substring(0, columns.Length - 2);
            par = par.Substring(0, par.Length - 2);

            var sql = new StringBuilder("INSERT INTO ");
            sql.Append(this.TableName);
            sql.Append("(");
            sql.Append(columns);
            sql.Append(") VALUES(");
            sql.Append(par);
            sql.Append(")");

            Database.Connection.ExecuteNonQuery(sql.ToString(), param_list);
            // AUTO_INCREMENT使用時は設定されたIDを主キーにセット
            if (this.UseAutoIncrement)
            {
                var id = int.Parse(Database.Connection.ExecuteScalar("SELECT last_insert_id()").ToString());
                var p = ty.GetProperty(this.KeyInfomation.First().Key);
                p.SetValue(this, id);
            }

            return true;
        }
        #endregion
    }
}
