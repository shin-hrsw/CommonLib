﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CommonLib.Base
{
    /// <summary>
    /// Modelの基底クラス
    /// </summary>
    /// <remarks>
    /// データベースのテーブルと1対1で対応(テーブルひとつにつきModelひとつ)
    /// テーブルの列はプロパティに対応するので名前、データ型を一致させること
    /// </remarks>
    public abstract class ModelBase
    {
        // 制御で使用するプロパティのリスト
        // "NotTableColumnAttribute"のような属性を作って制御してもいいが…
        private List<string> control_properties = new List<string>()
        { "TableName", "KeyInfomation", "UseAutoIncrement" };

        protected bool is_new = true;       // 新規データの場合にtrue
        protected bool is_changed = false;  // 変更された場合にtrue
        protected bool is_deleted = false;  // 削除された場合にtrue
        protected string error;       // エラーが発生した場合に設定

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

            try
            {
                Database.Connection.ExecuteNonQuery(sql.ToString(), param_list);
                // AUTO_INCREMENT使用時は設定されたIDを主キーにセット
                if (this.UseAutoIncrement)
                {
                    var id = int.Parse(Database.Connection.ExecuteScalar("SELECT last_insert_id()").ToString());
                    var p = ty.GetProperty(this.KeyInfomation.First().Key);
                    p.SetValue(this, id);
                }
            }
            catch (Exception ex)
            {
                this.error = ex.Message;
                return false;
            }

            return true;
        }

        private bool Change()
        {
            Type ty = this.GetType();
            // プロパティから列のリストを作成
            // 制御用プロパティは除外
            // 主キーはWhere句で使用するので対象にする
            var props = ty.GetProperties().Where(x =>
            {
                if (this.control_properties.Contains(x.Name)) { return false; }
                return true;
            });

            string set_clause = " SET ";
            string where_clause = "";
            bool is_first = true;
            var param_list = new List<Database.SQLParameter>();
            foreach(var p in props)
            {
                if (this.KeyInfomation.ContainsKey(p.Name))
                {
                    where_clause += is_first ? "WHERE " : " AND ";
                    where_clause += p.Name + " = @" + p.Name;

                    is_first = false;
                }
                else { set_clause += p.Name + " = @" + p.Name + ", "; }

                param_list.Add(new Database.SQLParameter(p.Name, p.GetType(), p.GetValue(this)));
            }
            set_clause = set_clause.Substring(0, set_clause.Length - 2);

            var sql = new StringBuilder("UPDATE ");
            sql.Append(this.TableName);
            sql.Append(set_clause);
            sql.Append(where_clause);

            Database.Connection.ExecuteNonQuery(sql.ToString(), param_list);

            return true;
        }
        #endregion

        #region メソッド(protected)
        /// <summary>
        /// テーブル検索
        /// </summary>
        /// <remarks>
        /// KeyInformationで指定されているプロパティの値を使って検索する
        /// </remarks>
        protected void Retrieve()
        {
            // KeyInformationが登録されていない場合は例外を飛ばす
            // キー情報無しで処理をするとテーブルのデータが全件取得されてしまうSQLになる
            if (this.KeyInfomation.Count == 0)
            {
                throw new InvalidOperationException("キー情報が設定されていません");
            }

            bool is_first = true;
            string where_clause = "";
            var param_list = new List<Database.SQLParameter>();
            var sql = new StringBuilder("SELECT * FROM ");
            sql.Append(this.TableName);
            foreach (var kvp in this.KeyInfomation)
            {
                // kvp.Key…主キーの列名、kvp.Value…主キーのデータ型
                where_clause += is_first ? " WHERE " : " AND ";
                where_clause += kvp.Key + " = @" + kvp.Key;

                var prop = this.GetType().GetProperty(kvp.Key);
                var value = prop.GetValue(this);
                // 主キーに値が設定されていない場合は例外を飛ばす
                if(value == null)
                {
                    throw new ArgumentException("主キーに値が設定されていません");
                }

                param_list.Add(
                    new Database.SQLParameter("@" + kvp.Key, kvp.Value, value));
                is_first = false;
            }
            sql.Append(where_clause);

            var dt = Database.Connection.ExecuteQuery(sql.ToString(), param_list);
            if(dt.Rows.Count == 1)
            {
                var row = dt.Rows[0];
                // プロパティに値を設定していく。キー情報と制御用プロパティは除外する
                var prop = this.GetType().GetProperties().Where(x =>
                {
                    if (this.KeyInfomation.ContainsKey(x.Name)) { return false; }
                    if (this.control_properties.Contains(x.Name)) { return false; }
                    return true;
                });
                foreach (var p in prop)
                {
                    var ty = p.GetType();
                    if (ty == typeof(string)) { p.SetValue(this, row.Field<string>(p.Name)); }
                    else if (ty == typeof(bool)) { p.SetValue(this, row.Field<bool>(p.Name)); }
                    else if (ty == typeof(int)) { p.SetValue(this, row.Field<int>(p.Name)); }
                    else if (ty == typeof(int?)) { p.SetValue(this, row.Field<int?>(p.Name)); }
                    else if (ty == typeof(long)) { p.SetValue(this, row.Field<long>(p.Name)); }
                    else if (ty == typeof(long?)) { p.SetValue(this, row.Field<long?>(p.Name)); }
                    else if (ty == typeof(decimal)) { p.SetValue(this, row.Field<decimal>(p.Name)); }
                    else if (ty == typeof(decimal?)) { p.SetValue(this, row.Field<decimal?>(p.Name)); }
                    else if (ty == typeof(DateTime)) { p.SetValue(this, row.Field<DateTime>(p.Name)); }
                    else if (ty == typeof(DateTime?)) { p.SetValue(this, row.Field<DateTime?>(p.Name)); }
                    else if (ty == typeof(byte[])) { p.SetValue(this, row.Field<byte[]>(p.Name)); }
                    else { throw new ApplicationException("予期しないデータ型です"); }
                }
            }
            else
            {
                throw new KeyNotFoundException("指定されたデータが存在しません");
            }
        }
        #endregion
    }
}
