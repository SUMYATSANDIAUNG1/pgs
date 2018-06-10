﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.IO;
using  System.Runtime.InteropServices;
using System.Net;

namespace LTA_PGS_20
{
    class fn_global
    {
        static string magicchar = "\\";

        #region
        public static bool fn_checkbit(int base_num)
        {
            if (Tools.fn_check_bit(1, base_num))
                return true;
            for (int i = 1; i < 18; i++)
            {
                if (Tools.fn_check_bit(i * 2, base_num))
                    return true;
            }
            return false;
        }

        public static string fn_stripslash(ref string a_str)
        {
            int len, state;
            string str1;

            string result = "";
            state = 0;
            len = a_str.Length;
            for (int i = 0; i < len; i++)
            {
                str1 = a_str[i].ToString();
                if (state == 1)
                {
                    if (str1 == magicchar)
                        result = result + str1;
                    else
                    {
                        result = result + str1;
                        state = 0;
                    }
                }
                else
                {
                    if (str1 == magicchar)
                        state = 1;
                    else
                        result = result + str1;
                }
            }
            a_str = "";

            return result;
        }
        public static string fn_rawurlencode(string a_str)
        {
            string b;
            int i = 0;
            string result = "";

            while (i < a_str.Length)
            {
                b = a_str.Substring(i, 1);
                i++;
                if (b == "")
                    break;
                if ((b[0] >= 'A') && (b[0] <= 'Z'))
                {
                    result += b;
                }
                else if ((b[0] >= 'a') && (b[0] <= 'z'))
                {
                    result += b;
                }
                else if ((b[0] >= '0') && (b[0] <= '9'))
                {
                    result += b;
                }
                else
                {
                    byte a = (byte)b[0];
                    result += string.Format("%{0:X}", a);
                }

            }
            return result;
        }

        public static string fn_rawurldecode(string a_str)
        {
            string b, str, str2;
            int i;

            string result = "";
            i = 0;
            str = "";
            while (i<a_str.Length)
            {
                b = a_str.Substring(i, 1);
                if (b == "")
                    break;
                if (b == "%")
                {
                    str2 = a_str.Substring(i + 1, 2);
                    char[] ss = Tools.hexstr_to_array(str2);
                    str2 = Tools.array_to_str(ss, ss.Length);
                    i = i + 3;
                    str = str + str2;
                }
                else
                {
                    str = str + b;
                    i = i + 1;
                }
            }
            result = str;
            return result;
        }
        public static string fn_txt_getvalue(string[] a_strs,string a_key)
        {
            string str;
            int i_pos, i_pos2;

            string r = "";
            for (int i = 0; i < a_strs.Length; i++)
            {
                str = a_strs[i];
                i_pos = str.IndexOf(a_key);
                if (i_pos >= 0)
                {
                    i_pos2 = str.IndexOf("=");
                    if (i_pos2 >= 0)
                    {
                        str = str.Substring(i_pos2 + 1, str.Length - i_pos2 - 1);                                                
                        r = str.Trim();
                        return r;
                    }
                }
            }
            return r;
        }
        public static string fn_txt_getvalue(ref List<string> a_strs, string a_key)
        {
            string str;
            int i_pos, i_pos2;

            string r = "";
            for (int i = 0; i < a_strs.Count; i++)
            {
                str = a_strs[i];
                i_pos = str.IndexOf(a_key);
                if (i_pos >= 0)
                {
                    i_pos2 = str.IndexOf("=");
                    if (i_pos2 >= 0)
                    {
                        str = str.Substring(i_pos2+1, str.Length - i_pos2-1);
                        str = fn_rawurldecode(str);
                        str=fn_stripslash(ref str);
                        r = str.Trim();
                        return r;
                    }
                }
            }
            return r;
        }
        public static string fn_getwordbysign(ref string a_str,string a_sign)
        {
            int i;
            string r = a_str;
            i = a_str.IndexOf(a_sign);
            if (i > 0)
            {
                r = a_str.Substring(0, i);
                a_str = a_str.Substring(i + 1, a_str.Length - i - 1);
            }
            else
            {
                a_str = "";
            }
            return r;
        }

        public static List<String> fn_str2strlist(string a_str)
        {
            List<String> a_strs = new List<string>();
            string str;

            while (true)
            {
                str = fn_getwordbysign(ref a_str, inc.split_sign);
                str = str.Trim();
                if (str == "")
                    break;
                a_strs.Add(str);
            }
            return a_strs; 
        }
        public string pack_str2text(string a_str)
        {
            return "";
        }
        public static string fn_str_getvalue(ref string a_strs)
        {
            string str;
            int i_pos2;

            string r = "";

            i_pos2 = a_strs.IndexOf(",");
            if (i_pos2 >= 0)
            {
                str = a_strs.Substring(0, i_pos2);
                a_strs = a_strs.Substring(i_pos2 + 1, a_strs.Length - i_pos2 - 1);
                r = str.Trim();
                return r;
            }
            else
            {
                r = a_strs.Trim();
                a_strs = "";
            }


            return r;
        }

        public static int get_status(int vmsid)
        {
            inc.db_control.SQL = "select fd_msgid,fd_delete from tb_alarm where fd_gdmcms='" + vmsid + "'order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            int r = -1;
            if (MyReader != null)
            {
                while (MyReader.Read())
                {
                    //ack = MyReader["fd_delete"] == DBNull.Value ? 0 : (int)(long)MyReader["fd_delete"];

                    r = MyReader["fd_delete"] == DBNull.Value ? -1 : (int)MyReader["fd_msgid"];
                    if (r == 2)
                        return r;
                }
            }
            return r;
        }

        /*public static int get_status(int vmsid,ref int ack)
        {
            inc.db_control.SQL = "select fd_msgid,fd_delete from tb_alarm where fd_gdmcms='" + vmsid + "'order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            int r = -1;
            if (MyReader != null)
            {
                while (MyReader.Read())
                {
                    //DateTime t_n = DateTime.Now;
                    //DateTime t = MyReader["fd_time_ack"] == DBNull.Value ? t_n : (DateTime)MyReader["fd_time_ack"];
                    
                    //ack = t == t_n ? 0 : 1;

                    //if (ack == 1)
                    //    ack = ((t_n - t).TotalDays > 7200) ? 0 : 1;
                    ack = MyReader["fd_delete"] == DBNull.Value ? 0 : (int)(long)MyReader["fd_delete"];

                    r = MyReader["fd_delete"] == DBNull.Value ? -1 : (int)MyReader["fd_msgid"];
                    if (r == 2)
                        return r;
                }
            }
            return r;
        }*/
        #endregion

        #region vmss control

        public static void load_vmss_config()
        {
            inc.db_control.SQL = "select * from tb_gdm where fd_delete=0 order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            if (MyReader != null)
            {
                inc._vms = new inc.vms_str[MyReader.RecordsAffected];
                inc.VMS = new object[MyReader.RecordsAffected];
                int i = 0;

                while (MyReader.Read())
                {
                    inc.VMS[i] = (string)MyReader["fd_name"];
                    inc._vms[i]._id = (int)MyReader["fd_id"];
                    inc._vms[i].name = (string)MyReader["fd_name"];
                    inc._vms[i].lcoation = (string)MyReader["fd_location"];
                    inc._vms[i].x = (int)MyReader["fd_x"];
                    inc._vms[i].y = (int)(Int64)MyReader["fd_y"];

                    Point _point = new Point(inc._vms[i].x, inc._vms[i].y);
                    _point = fn_1024tocurrent(_point);
                    inc._vms[i].x = _point.X;
                    inc._vms[i].y = _point.Y;

                    inc._vms[i].area = (int)(Int64)MyReader["fd_area"];
                    inc._vms[i].dim = (int)(Int64)MyReader["fd_dim"];
                    inc._vms[i].flash = (int)(byte)MyReader["fd_flash"];
                    try
                    {
                        inc._vms[i].timer = (int)(long)MyReader["fd_timer"];
                    }
                    catch (Exception e1) { MessageBox.Show(e1.Message); }
                    inc._vms[i].status = -1;
                    //inc._vms[i].ack = 0;
                    //inc._vms[i].status = get_status(inc._vms[i]._id, ref inc._vms[i].ack);
                    inc._vms[i].status = get_status(inc._vms[i]._id);
                    i++;
                }
            }
        }

        public static int fn_vms_id2index(int id)
        {
            for (int i = 0; i < inc._vms.Length; i++)
            {
                if (inc._vms[i]._id == id)
                    return i;
            }

            return -1;
        }
       

        public static int fn_vms_name2index(string name)
        {
            for (int i = 0; i < inc._vms.Length; i++)
            {
                //int _compare = name.CompareTo(inc._vms[i].name);
                if (inc._vms[i].name.Trim() == name.Trim())
                    return i;
            }

            return -1;
        }
        #endregion

        #region cmss control
        public static void load_cmss_config()
        {
            inc.db_control.SQL = "select * from tb_carpark order by fd_id";
            OdbcDataReader MyReader =  inc.db_control.SQLExecuteReader();            

            if (MyReader != null)
            {
                inc.Carpark = new inc.carpark[MyReader.RecordsAffected];
                inc.carparkobject = new System.Object[MyReader.RecordsAffected];
                int i = 0;
                while (MyReader.Read())
                {
                    inc.Carpark[i].id = (Int32)MyReader["fd_id"];
                    inc.Carpark[i].name =(String)MyReader["fd_name"];
                    inc.Carpark[i].name2 = (String)MyReader["fd_name2"];
                    inc.Carpark[i].ip = (String)MyReader["fd_ip"];
                    try
                    {
                        inc.Carpark[i].limit = (Int32)MyReader["fd_lotslimit"];
                    }
                    catch (Exception e1)
                    {
                        MessageBox.Show(e1.Message);
                    }
                    inc.Carpark[i].map_point.X = (int)(Int64)MyReader["fd_x"];
                    inc.Carpark[i].map_point.Y = (int)(Int64)MyReader["fd_y"];

                    Point _point = new Point(inc.Carpark[i].map_point.X, inc.Carpark[i].map_point.Y);
                    _point = fn_1024tocurrent(_point);
                    inc.Carpark[i].map_point.X = _point.X;
                    inc.Carpark[i].map_point.Y = _point.Y;

                    inc.Carpark[i].map_id = (Int32)MyReader["fd_map"];
                    inc.Carpark[i].x_out = (int)(Int64)MyReader["fd_xout"];
                    inc.Carpark[i].y_out = (int)(Int64)MyReader["fd_yout"];

                    inc.Carpark[i].f_time = (String)MyReader["fd_ftime"];
                    inc.Carpark[i].t_time = (String)MyReader["fd_ttime"];

                    inc.Carpark[i].area = (int)(Int64)MyReader["fd_area"];
                    inc.Carpark[i].status = -1;
                    //inc.Carpark[i].ack = 0;
                    //inc.Carpark[i].status = get_status(10000 + inc.Carpark[i].id, ref inc.Carpark[i].ack);
                    inc.Carpark[i].status = get_status(10000 + inc.Carpark[i].id);
                    inc.Carpark[i].latitude = (string)MyReader["fd_latitude"];
                    inc.Carpark[i].longtitude = (string)MyReader["fd_longtitude"];

                    inc.carparkobject[i] = inc.Carpark[i].name;


                    i++;
                }
            }
        }
        public static int fn_cms_id2index(int id)
        {
            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                if (inc.Carpark[i].id == id)
                    return i;
            }

            return -1;
        }
        public static int fn_cms_name2index(string name)
        {
            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                if (inc.Carpark[i].name == name)
                    return i;
            }

            return -1;
        }
        #endregion

        #region tb_user control
        public static void load_user_table()
        {
            inc.db_control.SQL = "select * from tb_users order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();
            if (MyReader != null)
            {
                inc.TB_user = new inc.users_str[MyReader.RecordsAffected];

                int i = 0;

                while (MyReader.Read())
                {
                    inc.TB_user[i].id = (int)((Int64)MyReader["fd_id"]);
                    inc.TB_user[i].name = (String)MyReader["fd_username"];
                    inc.TB_user[i].pwd = (String)MyReader["fd_password"];
                    inc.TB_user[i].level = (Int32)MyReader["fd_level"];
                    inc.TB_user[i].times = (Int32)MyReader["fd_trytimes"];
                    if (MyReader["fd_lasttime"] != DBNull.Value)
                        inc.TB_user[i].lasttime = (DateTime)MyReader["fd_lasttime"];
                    i++;
                }
            }
        }
        public static int fn_user_id2index(int id)
        {
            for (int i = 0; i < inc.TB_user.Length; i++)
            {
                if (inc.TB_user[i].id == id)
                    return i;
            }

            return -1;
        }

        public static int fn_user_name2index(string name)
        {
            for (int i = 0; i < inc.TB_user.Length; i++)
            {
                if (inc.TB_user[i].name == name)
                    return i;
            }

            return -1;
        }

        #endregion

        #region tb_area control
        public static void load_area_table()
        {
            inc.db_control.SQL = "select * from tb_area order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();
            if (MyReader != null)
            {
                inc.tb_area = new inc.area[MyReader.RecordsAffected];

                int i = 0;
                inc.Area = new object[MyReader.RecordsAffected];

                while (MyReader.Read())
                {
                    //Type aa = MyReader.GetFieldType(2);
                    inc.tb_area[i].fd_id = (int)(Int64)MyReader["fd_id"];

                    inc.tb_area[i].x = (int)MyReader["fd_x"];
                    inc.tb_area[i].y = (int)MyReader["fd_y"];
                    Point _point = new Point(inc.tb_area[i].x, inc.tb_area[i].y);
                    _point = fn_1024tocurrent(_point);
                    inc.tb_area[i].x = _point.X;
                    inc.tb_area[i].y = _point.Y;

                    inc.tb_area[i].areaid = (int)MyReader["fd_areaid"];
                    inc.tb_area[i].name = (string)MyReader["fd_name"];
                    inc.tb_area[i].map_name = (string)MyReader["fd_image"];
                    inc.tb_area[i].show = (int)MyReader["fd_show"];
                    inc.Area[i] = inc.tb_area[i].name;

                    i++;
                }
            }            
        }

        public static int fn_area_name2index(string name)
        {
            for (int i = 0; i < inc.tb_area.Length; i++)
            {
                if (inc.tb_area[i].name == name)
                    return i;
            }

            return -1;
        }
        public static int fn_area_id2index(int id)
        {
            for (int i = 0; i < inc.tb_area.Length; i++)
            {
                if (inc.tb_area[i].areaid == id)
                    return i;
            }

            return -1;
        }


        public static void udpate_users_password_time(int id)
        {
            inc.db_control.SQL = "update tb_users set fd_password_time='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'";
            inc.db_control.SQL += " where fd_id='" + id + "'";
            try
            {
                inc.db_control.SQLExecuteReader();
            }
            catch { }
        }
        #endregion

        #region treeview control
        /// <summary>
        /// if the vms already in vms group, the treeview will not include the VMS
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="vms_group"></param>
        public static void update_vms_treeview(TreeView treeView, string vms_group)
        {
            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            string[] vms_list = vms_group.Split(',');            

            tArea.Columns.Add("AreaID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("AreaID");
            tVMSCMS.Columns.Add("Name");

            for (int i = 0; i < inc.tb_area.Length; i++)
            {
                tArea.Rows.Add();
                tArea.Rows[i]["AreaID"] = inc.tb_area[i].areaid;
                tArea.Rows[i]["Name"] = inc.tb_area[i].name;
            }

            int rows_index = -1;

            for (int i = 0; i < inc._vms.Length; i++)
            {
                bool already_in = false;
                
                for(int j=0;j<vms_list.Length;j++)
                {
                    if (vms_list[j] == inc._vms[i].name)
                    {
                        already_in = true;
                    }
                }
                
                if (!already_in)
                {
                    tVMSCMS.Rows.Add();
                    rows_index++;
                    tVMSCMS.Rows[rows_index]["AreaID"] = inc._vms[i].area;
                    tVMSCMS.Rows[rows_index]["Name"] = inc._vms[i].name;
                }
            }

            set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "AreaID", "Name", "Name");
        }

        public static void load_vms_treeview(TreeView treeView, int area)
        {
            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            tArea.Columns.Add("AreaID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("AreaID");
            tVMSCMS.Columns.Add("Name");

            if (area >= 0)
            {
                tArea.Rows.Add();
                tArea.Rows[0]["AreaID"] = inc.tb_area[area].areaid;
                tArea.Rows[0]["Name"] = inc.tb_area[area].name;

                int _count = 1;
                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    if (area != i)
                    {
                        tArea.Rows.Add();
                        tArea.Rows[_count]["AreaID"] = inc.tb_area[i].areaid;
                        tArea.Rows[_count]["Name"] = inc.tb_area[i].name;
                        _count++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    tArea.Rows.Add();
                    tArea.Rows[i]["AreaID"] = inc.tb_area[i].areaid;
                    tArea.Rows[i]["Name"] = inc.tb_area[i].name;
                }
            }

            for (int i = 0; i < inc._vms.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[i]["AreaID"] = inc._vms[i].area;
                tVMSCMS.Rows[i]["Name"] = inc._vms[i].name;
            }            

            if (area == -1)
                set_treeview(treeView, "LTA PGS", tArea, null, "", "Name", "");
            else
                set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "AreaID", "Name", "Name");
        }
        public static void load_vms_cms_treeview(TreeView treeView,int area)
        {            
            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            tArea.Columns.Add("AreaID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("AreaID");
            tVMSCMS.Columns.Add("Name");

            if (area >= 0)
            {
                int tree_index = 0;
                tArea.Rows.Add();
                tArea.Rows[tree_index]["AreaID"] = inc.tb_area[area].areaid;
                tArea.Rows[tree_index]["Name"] = inc.tb_area[area].name;
                tree_index++;

                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    if (area != i)
                    {
                        tArea.Rows.Add();
                        tArea.Rows[tree_index]["AreaID"] = inc.tb_area[i].areaid;
                        tArea.Rows[tree_index]["Name"] = inc.tb_area[i].name;
                        tree_index++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    tArea.Rows.Add();
                    tArea.Rows[i]["AreaID"] = inc.tb_area[i].areaid;
                    tArea.Rows[i]["Name"] = inc.tb_area[i].name;
                }
            }

            int count = 0;
            for (int i = 0; i < inc._vms.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[count]["AreaID"] = inc._vms[i].area;
                tVMSCMS.Rows[count]["Name"] = inc._vms[i].name;
                count++;
            }

            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[count]["AreaID"] = inc.Carpark[i].area;
                tVMSCMS.Rows[count]["Name"] = inc.Carpark[i].name;
                count++;
            }

            if (area==-1)
                set_treeview(treeView, "LTA PGS", tArea, null, "", "Name", "");
            else
                set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "AreaID", "Name", "Name"); 
        }

        public static void load_cms_treeview(TreeView treeView, int area)
        {
            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            tArea.Columns.Add("AreaID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("AreaID");
            tVMSCMS.Columns.Add("Name");

            if (area >= 0)
            {
                int tree_index = 0;
                tArea.Rows.Add();
                tArea.Rows[tree_index]["AreaID"] = inc.tb_area[area].areaid;
                tArea.Rows[tree_index]["Name"] = inc.tb_area[area].name;
                tree_index++;

                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    if (area != i)
                    {
                        tArea.Rows.Add();
                        tArea.Rows[tree_index]["AreaID"] = inc.tb_area[i].areaid;
                        tArea.Rows[tree_index]["Name"] = inc.tb_area[i].name;
                        tree_index++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    tArea.Rows.Add();
                    tArea.Rows[i]["AreaID"] = inc.tb_area[i].areaid;
                    tArea.Rows[i]["Name"] = inc.tb_area[i].name;
                }
            }

            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[i]["AreaID"] = inc.Carpark[i].area;
                tVMSCMS.Rows[i]["Name"] = inc.Carpark[i].name;
            }

            if (area == -1)
                set_treeview(treeView, "LTA PGS", tArea, null, "", "Name", "");
            else
                set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "AreaID", "Name", "Name");
        }

        public static void load_msg_treeview(TreeView treeView)
        {
            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            tArea.Columns.Add("VMSID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("VMSID");
            tVMSCMS.Columns.Add("Name");

            tArea.Rows.Add();
            tArea.Rows[0]["VMSID"] = "0";
            tArea.Rows[0]["Name"] = "All";

            //for (int i = 0; i < inc._vms.Length; i++)
            for (int i = 0; i < inc.Pre_Msg.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[i]["VMSID"] = inc.Pre_Msg[i].gdm;
                tVMSCMS.Rows[i]["Name"] = inc.Pre_Msg[i].name;
            }


            set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "VMSID", "Name", "Name");
        }

        public static void load_msg_treeview(TreeView treeView, string vms_group)
        {
            if(vms_group==null)
                return;
            if(vms_group=="")
                return;
            string[] vms_list = vms_group.Split(',');            

            DataTable tArea = new DataTable();
            DataTable tVMSCMS = new DataTable();

            tArea.Columns.Add("VMSID");
            tArea.Columns.Add("Name");

            tVMSCMS.Columns.Add("VMSID");
            tVMSCMS.Columns.Add("Name");

            int _count = 0;
            for (int i = 0; i < vms_list.Length; i++)
            {
                if (vms_list[i].Trim() != "")
                {
                    tArea.Rows.Add();
                    tArea.Rows[i]["VMSID"] = inc._vms[fn_vms_name2index(vms_list[i])]._id.ToString();
                    tArea.Rows[i]["Name"] = vms_list[i];
                }
            }
            tArea.Rows.Add();
            tArea.Rows[tArea.Rows.Count - 1]["VMSID"] = "0";
            tArea.Rows[tArea.Rows.Count - 1]["Name"] = "All";

            for (int i = 0; i < inc.Pre_Msg.Length; i++)
            {
                tVMSCMS.Rows.Add();
                tVMSCMS.Rows[i]["VMSID"] = inc.Pre_Msg[i].gdm;
                tVMSCMS.Rows[i]["Name"] = inc.Pre_Msg[i].name;
            }


            set_treeview(treeView, "LTA PGS", tArea, tVMSCMS, "VMSID", "Name", "Name");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeView"> Edit TreeView Control Handle</param>
        /// <param name="name">Root Node name</param>
        /// <param name="tCustomers">first node table</param>
        /// <param name="tOrders">second node table</param>
        /// <param name="key">key of the 2 tables</param>
        /// <param name="first">first table display field</param>
        /// <param name="second">seconed table display field</param>
        public static void set_treeview(TreeView treeView, string name, DataTable tCustomers, DataTable tOrders, string key, string first,string second)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            TreeNode rootNode = new TreeNode(name, 0, 0);
            treeView.Nodes.Add(rootNode);

            int len = 0;
            foreach (DataRow rowcust in tCustomers.Rows)
            {
                TreeNode tncust = new TreeNode(rowcust[first].ToString());
                bool child_value = false;
                rootNode.Nodes.Add(tncust);
                if (tOrders != null)
                {                    
                    string filt = key + "='" + rowcust[key] + "'";                
                    DataRow[] order = tOrders.Select(filt, second + " ASC");
                
                    foreach (DataRow roworder in order)
                    {
                        string _temp = roworder[second].ToString();
                        if (len < _temp.Length)
                            len = _temp.Length;
                        TreeNode tnorder = new TreeNode(_temp);
                        tncust.Nodes.Add(tnorder);
                        child_value = true;
                    }
                    if(!child_value)
                        rootNode.Nodes.Remove(tncust);
                }
            }


            len = len*12 + 20;
            if (len > treeView.Width)
                treeView.Width = len;
            treeView.ExpandAll();
            treeView.EndUpdate();            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeView">TreeView Control Handl</param>
        /// <param name="x">x</param>
        /// <param name="y">y</param>
        public static void show_treeview_location(TreeView treeView, int x, int y)
        {
            treeView.Height = 100;
            treeView.Width = 240;
            if (treeView.Parent != null)
            {
                if (treeView.Width + x > treeView.Parent.Width)
                {
                    x -= treeView.Width + x - treeView.Parent.Width;
                }
                if (treeView.Height + y > treeView.Parent.Height)
                {
                    treeView.Height -= treeView.Height + y - treeView.Parent.Height;
                }
            }
            treeView.Left = x;
            treeView.Top = y;
            treeView.Visible = true;
            treeView.BringToFront();
            treeView.Focus();
        }

        public static void Auto_close_treeview(TreeView treeView,ref string p_text,ref string n_text)
        {
            if (treeView.SelectedNode.Parent != null)
                p_text = treeView.SelectedNode.Parent.Text;
            n_text = treeView.SelectedNode.Text;
            if (treeView.SelectedNode.Nodes.Count == 0)
            {
                treeView.Visible = false;
            }
        }

        #endregion

        #region tb_premessage
        public static void load_premessage()
        {
            inc.db_control.SQL = "select fd_id,fd_gdm,fd_color,fd_name,fd_line1,fd_line2,fd_line3,fd_line4,fd_line5,fd_line6 from tb_premsg order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            if (MyReader != null)
            {
                inc.Pre_Msg = new inc.pre_msg[MyReader.RecordsAffected];
                inc.Pre_Message = new object[MyReader.RecordsAffected];
                int i = 0;
                while (MyReader.Read())
                {
                    inc.Pre_Msg[i].id = (int)(Int64)MyReader["fd_id"];
                    inc.Pre_Msg[i].gdm = (int)MyReader["fd_gdm"];
                    inc.Pre_Msg[i].color = (int)MyReader["fd_color"];
                    if (MyReader["fd_name"] != DBNull.Value)
                        inc.Pre_Msg[i].name = (string)MyReader["fd_name"];
                    if (MyReader["fd_line1"] != DBNull.Value)
                        inc.Pre_Msg[i].line1 = (string)MyReader["fd_line1"];
                    if (MyReader["fd_line2"] != DBNull.Value)
                        inc.Pre_Msg[i].line2 = (string)MyReader["fd_line2"];
                    if (MyReader["fd_line3"] != DBNull.Value)
                        inc.Pre_Msg[i].line3 = (string)MyReader["fd_line3"];
                    if (MyReader["fd_line4"] != DBNull.Value)
                        inc.Pre_Msg[i].line4 = (string)MyReader["fd_line4"];
                    if (MyReader["fd_line5"] != DBNull.Value)
                        inc.Pre_Msg[i].line5 = (string)MyReader["fd_line5"];
                    if (MyReader["fd_line6"] != DBNull.Value)
                        inc.Pre_Msg[i].line6 = (string)MyReader["fd_line6"];
                    inc.Pre_Message[i] = MyReader.GetString(3);
                    i++;
                }
            }
        }

        public static int fn_msg_name2index(string name)
        {
            for (int i = 0; i < inc.Pre_Msg.Length; i++)
            {
                if (inc.Pre_Msg[i].name == name)
                    return i;
            }

            return -1;
        }
        #endregion

        #region File Control
        public static string get_workdir(string folder)
        {
            string path = "";
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")
            {
                path += System.Configuration.ConfigurationSettings.AppSettings["LocalDisk"] + "\\\\";
                path += System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                if (folder != "")
                    path += "\\" + folder; 
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2")
            {
                path += System.Configuration.ConfigurationSettings.AppSettings["LocalDisk"] + "\\\\";
                path += System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                if (folder != "")
                    path += "\\" + folder;
            }
            else
            {
                path = Environment.CurrentDirectory + "\\temp";
            }
            return path;
        }
        public static bool fn_get_file(string path,string s_name,string n_name)
        {
            if ((System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")||(System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2"))
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                    return true;
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    http FileOne = new http();

                    FileOne.FileLocalPath = Environment.CurrentDirectory + "\\temp\\" + n_name;
                    if (File.Exists(FileOne.FileLocalPath))
                        File.Delete(FileOne.FileLocalPath);

                    FileOne.Url = System.Configuration.ConfigurationSettings.AppSettings["webserviceurl"] + "/" + path + "/" + s_name;
                    
                    FileOne.Connect();
                    if (FileOne.IsFileExist)
                    {
                        FileOne.DownloadFile();
                        FileOne.Close();
                        return true;
                    }
                    else
                    {
                        FileOne.Close();
                        return false;
                    }                    
                }
                catch 
                {
                    //FileOne.Close();
                    return false; 
                }                
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];
                string ltapgs = System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                if (path != "")
                    path = "\\" + path;

                FTP ftp1 = new FTP(ftp_svr, ltapgs + path, user, pwd, int.Parse(port));

                try
                {
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {
                        string _path = Environment.CurrentDirectory + "\\temp";
                        Tools.make_folder(_path);
                        // Thread.Sleep(500);
                        ftp1.Get(s_name, _path, n_name);
                        ftp1.DisConnect();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    //string s = e.Message;
                    ftp1.DisConnect();
                    MessageBox.Show(e.Message);
                }

                return false;
            }
        }
        public static bool fn_put_file(string path, string n_name)
        {
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                    return true;
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                    return true;
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    http FileOne = new http();

                    MyWsitMsg.MyWaitMSG msg = new MyWsitMsg.MyWaitMSG("Connecting...", inc.images_path + "\\" + "waiting.gif");            
                    msg.MsgShow();

                    Thread.Sleep(200);

                    bool _r = FileOne.Upload_Request(System.Configuration.ConfigurationSettings.AppSettings["webserviceurl"]+"/upload.php"
                        , path
                        , Environment.CurrentDirectory + "\\temp\\" + n_name);

                    try { msg.MsgClose(); }
                    catch { }

                    return _r;

                }
                catch
                {
                    //FileOne.Close();
                    return false;
                }
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];
                string ltapgs = System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                if (path != "")
                    path = "\\" + path;

                FTP ftp1 = new FTP(ftp_svr, ltapgs + path, user, pwd, int.Parse(port));

                try
                {
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {
                        string _path = Environment.CurrentDirectory + "\\temp";
                        ftp1.Put(_path + "\\" + n_name);
                        ftp1.DisConnect();
                        return true;
                    }

                }
                catch (Exception e)
                {
                    ftp1.DisConnect();
                    string s = e.Message;
                }

                return false;
            }
        }
        public static bool fn_file_exist(string path, string n_name)
        {
            string _workdir = fn_global.get_workdir(path);

            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    string _file = _workdir + "\\" + n_name;

                    if(File.Exists(_file))
                        return true;
                    else
                        return false;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2")
            {
                if (!inc.remote_disk.Connected)
                {
                    //MessageBox.Show(_workdir + "\\" + n_name);
                    return false;
                }
                else
                {
                    string _file = _workdir + "\\" + n_name;
                    //MessageBox.Show(_file);

                    if (File.Exists(_file))
                        return true;
                    else
                        return false;
                }
            }
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    http FileOne = new http();

                    FileOne.FileLocalPath = Environment.CurrentDirectory + "\\temp\\" + n_name;
                    FileOne.Url = System.Configuration.ConfigurationSettings.AppSettings["webserviceurl"] + "/" + path + "/" + n_name;                    
                    FileOne.Connect();
                    if (FileOne.IsFileExist)
                    {                        
                        FileOne.Close();
                        return true;
                    }
                    else
                    {
                        FileOne.Close();
                        return false;
                    }
                }
                catch
                {
                    //FileOne.Close();
                    return false;
                }
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];                
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                try
                {                    
                    FTP ftp1 = new FTP(ftp_svr, path, user, pwd, int.Parse(port));
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {
                        if (ftp1.GetFileSize(n_name) >= 0)
                            return true;
                        else
                            return false;
                    }
                }
                catch { }
                return false;
            }
        }
        public static bool fn_file_delete(string path, string n_name)
        {
            string _workdir = fn_global.get_workdir(path);

            string ltapgs = System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
            if (path != "")
                path = "\\" + path;

            if ((System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1") || (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2"))
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    string _localpath = System.Configuration.ConfigurationSettings.AppSettings["LocalDisk"];

                    string _file = _workdir + "\\" + n_name;

                    if (File.Exists(_file))
                    {
                        try { File.Delete(_file); }
                        catch { }
                    }
                    return true;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                return true;
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                try
                {
                    FTP ftp1 = new FTP(ftp_svr, ltapgs + path, user, pwd, int.Parse(port));
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {
                        bool del = false;
                        try
                        {
                            if (ftp1.GetFileSize(n_name) >= 0)
                            {
                                del = true;
                            }
                        }
                        catch { }
                        if (del)
                            ftp1.Delete(n_name);
                        return true;
                    }
                }
                catch { }
                return false;
            }
        }
        #endregion

        #region map control
        public static void fn_load_map(int area_id, f_paintbox paint_box)
        {
            paint_box.mf_clearicon();
            string _vms_green = System.Configuration.ConfigurationSettings.AppSettings["VMS_OK"];
            string _vms_yellow = System.Configuration.ConfigurationSettings.AppSettings["VMS_Error"];
            string _vms_red = System.Configuration.ConfigurationSettings.AppSettings["VMS_Disconnected"];

            string _cms_green = System.Configuration.ConfigurationSettings.AppSettings["CMS_OK"];
            string _cms_red = System.Configuration.ConfigurationSettings.AppSettings["CMS_Disconnected"];

            for (int i = 0; i < inc._vms.Length; i++)
            {
                if (inc._vms[i].area == area_id)
                {
                    if ((inc._vms[i].status == 2) || (!inc.server_status_ok))
                        paint_box.mf_addicon(inc._vms[i].x, inc._vms[i].y, inc.images_path + "\\" + _vms_red, true, inc._vms[i]._id, false);
                    else if (inc._vms[i].status == -1)
                        paint_box.mf_addicon(inc._vms[i].x, inc._vms[i].y, inc.images_path + "\\" + _vms_green, false, inc._vms[i]._id, false);
                    else
                        paint_box.mf_addicon(inc._vms[i].x, inc._vms[i].y, inc.images_path + "\\" + _vms_yellow, true, inc._vms[i]._id, false);
                }
            }

            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                if (inc.Carpark[i].area == area_id)
                {
                    if ((inc.Carpark[i].status == -1) && (inc.server_status_ok))
                        paint_box.mf_addicon(inc.Carpark[i].map_point.X, inc.Carpark[i].map_point.Y, inc.images_path + "\\" + _cms_green, false, inc.Carpark[i].id + 10000, false);
                    else
                        paint_box.mf_addicon(inc.Carpark[i].map_point.X, inc.Carpark[i].map_point.Y, inc.images_path + "\\" + _cms_red, true, inc.Carpark[i].id + 10000, false);
                }
            }
        }

        public static void fn_refresh_mainmap(f_paintbox paint_box)
        {
            paint_box.mf_clearicon();
            string _location_icon_marina_green = System.Configuration.ConfigurationSettings.AppSettings["Icon_marina"];
            string _location_icon_marina_red = System.Configuration.ConfigurationSettings.AppSettings["Icon_marina_red"];

            string _location_icon_orchard_green = System.Configuration.ConfigurationSettings.AppSettings["Icon_orchard"];
            string _location_icon_orchard_red = System.Configuration.ConfigurationSettings.AppSettings["Icon_orchard_red"];

            string _location_icon_harbfront_green = System.Configuration.ConfigurationSettings.AppSettings["Icon_Harbfront"];
            string _location_icon_harbfront_red = System.Configuration.ConfigurationSettings.AppSettings["Icon_Harbfront_red"];

            for (int i = 0; i < inc.tb_area.Length; i++)
            {
                string _path = "";
                bool _have_alarm = check_location_status(inc.tb_area[i].areaid);//if have should flash

                if (!inc.server_status_ok) _have_alarm = true;

                if (i == 0)
                {                    
                    if(_have_alarm)
                        _path = _location_icon_marina_red;
                    else
                        _path = _location_icon_marina_green;                    
                }
                else if (i == 1)
                {
                    if (_have_alarm)
                        _path = _location_icon_orchard_red;
                    else
                        _path = _location_icon_orchard_green;
                    
                }
                else
                {
                    if (_have_alarm)
                        _path = _location_icon_harbfront_red;
                    else
                        _path = _location_icon_harbfront_green;
                }

                Bitmap _bitmap = (Bitmap)Bitmap.FromFile(inc.images_path + "\\"+ _path, false);
                inc.tb_area[i].map_size = new Size(_bitmap.Width, _bitmap.Height);

                paint_box.mf_addicon(inc.tb_area[i].x, inc.tb_area[i].y, inc.images_path + "\\"
                    + _path, _have_alarm, inc.tb_area[i].areaid, true);
            }
        }

        public static bool check_location_status(int area_id)
        {
            for (int i = 0; i < inc._vms.Length; i++)
            {
                if (inc._vms[i].area == area_id)
                {
                    //if ((inc._vms[i].status != -1) && (inc._vms[i].ack == 0))
                    if (inc._vms[i].status != -1)
                        return true;
                }
            }

            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                if (inc.Carpark[i].area == area_id)
                {
                    //if ((inc.Carpark[i].status != -1) && (inc.Carpark[i].ack == 0))
                    if (inc.Carpark[i].status != -1)
                        return true;
                }
            }
            return false;
        }
        public static void make_fix_map_pic(Size size )
        {
            size.Width -= 25;
            size.Height -= 30;
            for(int i=0;i<inc.tb_area.Length;i++)
            {
                string s_FileName1 = inc.images_path + "\\" + inc.tb_area[i].map_name;
                string t_FileName1 = inc.images_path + "\\temp\\" + inc.tb_area[i].map_name;
                Tools.copy_resize_image(size, s_FileName1, t_FileName1);
            }
            /*
            string s_FileName = inc.images_path + "\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_marina"];
            string t_FileName = inc.images_path + "\\temp\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_marina"];
            Tools.copy_resize_image(size, s_FileName, t_FileName);

            s_FileName = inc.images_path + "\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_orchard"];
            t_FileName = inc.images_path + "\\temp\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_orchard"];
            Tools.copy_resize_image(size, s_FileName, t_FileName);

            s_FileName = inc.images_path + "\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_Harbfront"];
            t_FileName = inc.images_path + "\\temp\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_Harbfront"];
            Tools.copy_resize_image(size, s_FileName, t_FileName);
            */
            string s_FileName = inc.images_path + "\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_singapore"];
            string t_FileName = inc.images_path + "\\temp\\" + System.Configuration.ConfigurationSettings.AppSettings["Map_singapore"];
            Tools.copy_resize_image(size, s_FileName, t_FileName);
        }
        #endregion

        #region tcp commands check
        public static bool fn_reload_ccs()
        {
            if (MessageBox.Show("Restart CCS Server?", "Restart CCS Server", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] == "1")
                    return true;
                string str = "<reload server>";
                string tcpserver = System.Configuration.ConfigurationSettings.AppSettings["TCPServerAdd"];
                int tcpserverport = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["TCPServerPort"]);

                int timeout = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["Update_Timeout"]);
                inc.tcp_client = new TCPClient(tcpserver, tcpserverport);
                inc.tcp_client.connect();
                if (inc.tcp_client.Connected)
                {
                    inc.tcp_client.SendBuffer(str + "\r\n");

                    string rev = "";
                    int r = inc.tcp_client.ReadBuffer(ref rev);
                }
                else
                {
                    MessageBox.Show("CCS SVR Down");
                    return false;
                }
                return true;
            }
            return false;
        }
        public static bool fn_tcp_send(string str)
        {
            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] == "1")
                return true;
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                string tcpserver = System.Configuration.ConfigurationSettings.AppSettings["TCPServerAdd"];
                int tcpserverport = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["TCPServerPort"]);

                int timeout = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["Update_Timeout"]);
                inc.tcp_client = new TCPClient(tcpserver, tcpserverport);
                inc.tcp_client.ReceviceTimeOut = timeout;

                inc.tcp_client.connect();
                if (inc.tcp_client.Connected)
                {
                    inc.tcp_client.SendBuffer(str + "\r\n");

                    MyWsitMsg.MyWaitMSG msg = new MyWsitMsg.MyWaitMSG("Connecting...", inc.images_path + "\\" + "waiting.gif");
                    msg.MsgShow();

                    string rev = "";
                    Application.DoEvents();
                    int r = inc.tcp_client.ReadBuffer(ref rev);


                    //MessageBox.Show(rev);

                    if (r == 0)
                    {
                        try { msg.MsgClose(); }
                        catch { }
                        MessageBox.Show("Time Out");
                        return false;
                    }
                    if (rev.IndexOf("busy")>=0)
                    {
                        try { msg.MsgClose(); }
                        catch { }
                        MessageBox.Show("Server Busy,please try later");
                        return false;
                    }
                    if (rev.IndexOf("wait") >= 0)
                    {                        
                        if (rev.IndexOf("finish") >= 0)
                        {
                            try { msg.MsgClose(); }
                            catch { }                            
                            return true;
                        }
                        else
                        {
                            r = 0;
                            r = inc.tcp_client.ReadBuffer(ref rev);
                            if (r == 0)
                            {
                                try { msg.MsgClose(); }
                                catch { }
                                MessageBox.Show("Time Out");
                                return false;
                            }
                            else if (rev.IndexOf("finish") >= 0)
                            {
                                //MessageBox.Show(rev);
                                try { msg.MsgClose(); }
                                catch { }
                                //MessageBox.Show("Server Busy,please try later");
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("CCS SVR Down");
                    return false;
                }
            }
            else
            {
                if (fn_file_delete("", "waiting.txt"))
                {
                    if (fn_file_delete("", "serverbusy.txt"))
                    {
                           string tcpserver = System.Configuration.ConfigurationSettings.AppSettings["TCPServerAdd"];
                        int tcpserverport = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["TCPServerPort"]);

                        int timeout = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["Update_Timeout"]);
                        inc.tcp_client = new TCPClient(tcpserver, tcpserverport);
                        inc.tcp_client.connect();
                        if (inc.tcp_client.Connected)
                        {
                            inc.tcp_client.SendBuffer(str + "\r\n");

                            //            inc.tcp_client.disconnect();
                            string rev = "";
                            int r = inc.tcp_client.ReadBuffer(ref rev);

                            int count = 0;
                            MyWsitMsg.MyWaitMSG msg = new MyWsitMsg.MyWaitMSG("Connecting...", inc.images_path + "\\" + "waiting.gif");
                            msg.MsgShow();
                            Thread.Sleep(500);
                            while (true)
                            {
                                Application.DoEvents();

                                if (count >= timeout)
                                {
                                    try { msg.MsgClose(); }
                                    catch { }
                                    MessageBox.Show("Time Out");
                                    return false;
                                }
                                if (fn_global.fn_file_exist("", "serverbusy.txt"))
                                {
                                    try { msg.MsgClose(); }
                                    catch { }
                                    MessageBox.Show("Server Busy,please try later");
                                    return false;
                                }
                                if (fn_file_exist("", "waiting.txt"))
                                {
                                    try { msg.MsgClose(); }
                                    catch { }
                                    return true;
                                }
                                Thread.Sleep(1000);
                                count++;
                            }
                        }
                        else
                        {
                            MessageBox.Show("CCS SVR Down");
                            return false;
                        }
                    }
                    return false;
                }
            }
            return false;
        }

        

        public static bool fn_cmd_check_finish(string path, string file_name,ref string str)
        {
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    if (fn_file_exist(path, file_name))
                    {
                        if (fn_get_file(path, file_name, file_name))
                        {
                            string _workdir = fn_global.get_workdir(path);
                            MyFile mf = new MyFile(_workdir + "\\" + file_name);
                            str = mf.read_file_all();
                            mf.close();
                            return true;
                        }
                    }
                    return false;
                }
                catch { return false; }
            }
            int timeout = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["Update_Timeout"]);

            int count = 0;
            MyWsitMsg.MyWaitMSG msg = new MyWsitMsg.MyWaitMSG("Connecting...", inc.images_path + "\\" + "waiting.gif");
            msg.MsgShow();
            Thread.Sleep(500);

            while (true)
            {
                if (count >= timeout)
                {
                    try { msg.MsgClose(); }
                    catch { }
                    MessageBox.Show("Time Out");
                    return false;
                }
                Application.DoEvents();
                if (fn_file_exist(path, file_name))
                {
                    if (fn_get_file(path, file_name, file_name))
                    {
                        string _workdir = fn_global.get_workdir(path);
                        MyFile mf = new MyFile(_workdir + "\\" + file_name);
                        str = mf.read_file_all();
                        mf.close();
                        try { msg.MsgClose(); }
                        catch { }
                        return true;
                    }
                }
              
                Thread.Sleep(1000);
                count++;                
            }            
        }
            
        #endregion

        #region cmd status form
        public static void fn_return_status(string str,string f_name,string title,int mode)
        {
            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] == "1")
            {
                if(f_name=="ledstatus.txt")
                    f_name = "ledstatus1.txt";
            }

            string[] str_s = str.Split('\n');
            if (str.IndexOf("isconnect") >= 0)
            {
                string temp = "VMS Not Connect";
                try
                {
                    temp = str_s[1].Replace("=", " ");

                }
                catch { }
                MessageBox.Show(temp);
            }
            else if (str_s.Length <= 3)
            {
                MessageBox.Show("Can not get value from VMS");
            }
            else
            {

                int c2 = str_s.Length;

                Form f = new Form();
                f.Width = 400;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.StartPosition = FormStartPosition.CenterScreen;
                f.Text = f_name;

                Label l1 = new Label();
                l1.AutoSize = true;
                l1.Text = title;
                l1.Left = (int)(f.Width / 2) - (int)(l1.Width / 2);
                l1.Top = 15;
                l1.Font = new Font(f.Font.FontFamily, 12);

                Label l2 = new Label();
                l2.AutoSize = true;
                l2.Text = "Time:" + str_s[0].Substring(str_s[0].IndexOf("=") + 1);
                l2.Left = 10;
                l2.Top = l1.Top + l1.Height + 5;

                int _height = 0;

                DataGridView d1 = new DataGridView();
                d1.Left = 10;
                d1.Top = l2.Top + l2.Height + 5;
                d1.Width = f.Width - 25;
                d1.Height = c2 * 25;
                if (d1.Height > 600) d1.Height = 600;
                d1.AllowUserToAddRows = false;
                d1.ReadOnly = true;
                
                if (mode == 0)
                {
                    d1.Columns.Add("Name", "Name");
                    d1.Columns.Add("Value", "Value");
                    d1.Columns["Name"].Width = 160;
                    d1.Columns["Value"].Width = 200;
                    for (int i = 0; i < c2 - 3; i++)
                    {
                        d1.Rows.Add();
                        try
                        {
                            d1.Rows[i].Cells["Name"].Value = str_s[i + 1].Substring(0, str_s[i + 1].IndexOf("="));
                            //d1.Rows[i].Cells["Name"].Value = str_s[i + 1];
                            d1.Rows[i].Cells["Value"].Value = str_s[i + 1].Substring(str_s[i + 1].IndexOf("=") + 1);
                        }
                        catch { }
                    }
                }
                else
                {
                    d1.Columns.Add("Name", "Status");
                    d1.Columns["Name"].Width = 290;
                    for (int i = 0; i < c2 - 3; i++)
                    {
                        d1.Rows.Add();
                        try
                        {
                            d1.Rows[i].Cells["Name"].Value = str_s[i + 1];
                        }
                        catch { }
                    }
                }

                _height = d1.Top + d1.Height;
                f.Controls.Add(d1);

                Button b1 = new Button();
                b1.Text = "OK";
                b1.Top = _height + 5;
                b1.Left = (int)(f.Width / 2) - (int)(b1.Width / 2);
                b1.Click += new EventHandler(f_b1_Click);

                f.Height = b1.Top + b1.Height + 50;

                f.Controls.Add(l1);
                f.Controls.Add(l2);

                f.Controls.Add(b1);

                f.ShowDialog();

            }            
        }
        private static void f_b1_Click(object sender, EventArgs e)
        {
            ((Button)sender).FindForm().Close();
        }
        #endregion


        #region tool functions
        public static Point fn_currentto1024(Point point)
        {
            int _w = Screen.PrimaryScreen.Bounds.Width;
            int _h = Screen.PrimaryScreen.Bounds.Height;

            if (System.Configuration.ConfigurationSettings.AppSettings["use_format_screen"] == "1")
            {                
                _w = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["format_width"]);
                _h = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["format_height"]);
            }

            Point new_point = new Point();
            new_point.X = (int)(point.X * 1024 / _w);
            new_point.Y = (int)(point.Y * 768 / _h);

            return new_point;
        }
        public static Point fn_1024tocurrent(Point point)
        {
            int _w = Screen.PrimaryScreen.Bounds.Width;
            int _h = Screen.PrimaryScreen.Bounds.Height;

            if (System.Configuration.ConfigurationSettings.AppSettings["use_format_screen"] == "1")
            {
                _w = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["format_width"]);
                _h = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["format_height"]);
            }

            Point new_point = new Point();
            new_point.X = (int)(point.X * _w / 1024);
            new_point.Y = (int)(point.Y * _h / 768);

            return new_point;
        }
        #endregion

        #region
        public static void load_logmessage()
        {
            inc.db_control.SQL = "select * from tb_logmessage";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();
            if (MyReader != null)
            {
                inc.tb_logmessage = new inc.logmsg[MyReader.RecordsAffected];

                int i = 0;

                while (MyReader.Read())
                {
                    inc.tb_logmessage[i].code = (string)MyReader["fd_code"];
                    inc.tb_logmessage[i].msg = (string)MyReader["fd_msg"];
                    inc.tb_logmessage[i].id = (int)MyReader["fd_id"];
                    i++;
                }
            }
        }

        private static string logmsg_code2msg(int id)
        {
            for (int i = 0; i < inc.tb_logmessage.Length; i++)
            {
                if (id == inc.tb_logmessage[i].id)
                    return inc.tb_logmessage[i].msg;
            }
            return "";
        }

        public static void log_operateion(int code,string para,int type)
        {
            try
            {
                string _msg = "";
                if (code != 0)
                    _msg = logmsg_code2msg(code) + " " + para;
                else
                    _msg = para;

                inc.db_control.SQL = inc.db_control.insert_table("tb_log", ""
                    , DateTime.Now.ToString("yyyy-MM-dd")
                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    , inc.currentuser.ToString()
                    , type.ToString()
                    , _msg);
                if (inc.db_control.SQLExecuteReader() == null)
                {
                    MessageBox.Show("Save Log Failed");
                }
            }
            catch (Exception e){ MessageBox.Show(e.Message); }
        }
        #endregion

        public static int fn_VMS_color2index(string info)
        {
            for (int i = 0; i < 3; i++)
            {
                if (info == inc.VMS_color_code[i].ToString())
                    return i;
            }
            return -1;
        }

        #region alarm sound
        [DllImport("kernel32.dll")] 
        private  static  extern  int  Beep(int  dwFreq  ,int  dwDuration)  ;

        public static void fn_alarm_sound()
        {
            int a = 0X7FF;  //发生频率(Hz)
            int b = 50;  //持续时间(毫秒)
            Beep(a, b);
        } 

        #endregion

        public static bool checkpwd(string id,string text)
        {
            inc.db_control.SQL = "select * from tb_pwdhistory where fd_userid='" + id + "' and fd_pwd='" + text + "'";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();
            string timestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            if (MyReader != null)
            {
                if (MyReader.Read())
                {
                    int _t = (int)MyReader["fd_times"];
                    if (_t >= 3)
                        return false;
                    else
                    {

                        inc.db_control.SQL = "update tb_pwdhistory set fd_time='" + timestr + "',fd_times='" + (_t + 1).ToString() + "' ";
                        inc.db_control.SQL = inc.db_control.SQL + "where fd_userid='" + id + "' and fd_pwd='" + text + "'";
                        inc.db_control.SQLExecuteReader();
                    }
                }
                else
                {
                    inc.db_control.SQL = inc.db_control.insert_table("tb_pwdhistory", "", text, "0", id, timestr);
                    inc.db_control.SQLExecuteReader();
                }
            }
            else
            {
                inc.db_control.SQL = inc.db_control.insert_table("tb_pwdhistory", "", text, "0", id, timestr);
                inc.db_control.SQLExecuteReader();
            }

            return true;
        }
        public static bool check_ea_status()
        {
            inc.db_control.SQL = "select * from tb_alarm where fd_sub_msgid=9901";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            if (MyReader != null)
            {
                if(MyReader.Read())
                {
                    return false;
                }
            }
                        
            return true;
        }
      
        public static bool check_firewall_status(int firewall)
        {

            string s = "switch to firewall " + firewall.ToString();            

            inc.db_control.SQL = "select  fd_msg from tb_log order by fd_time desc limit 1";

            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            if (MyReader != null)
            {
                if (MyReader.Read())
                {
                    string msg = MyReader.GetString(0);
                    if (msg != s)
                        return false;
                    else
                        return true;
                }
            }
            return false;
        }

        public static string fn_getmsgbyid(int id,int equipid)
        {
            int index = id - 1;
            string s = "";

            if ((equipid == 0) && (id == 1099)) 
                index = 0;
            else if ((equipid == 99) && (id == 99))
            {
                index = inc.error_msg.Length - 1;
            }
            else
            {                                
            }
            try { s = inc.error_msg[index]; }
            catch { }                
            return s;
        }

        /*private string get_msg_by_id(int id)
        {
            int aid = id;
            int _id = (id == 1099 ? 0 : (id - 1));
            if (aid == 99)
                _id = inc.error_msg.Length - 1;

            string s = "";
            try { s = inc.error_msg[_id]; }
            catch { }
            return s;
        }
        private string get_msg_by_id(int id)
        {
            int aid = id;
            int _id = (id == 1099 ? 0 : (id - 1));
            if (aid == 99)
                _id = inc.error_msg.Length - 1;

            string s = "";
            try { s = inc.error_msg[_id]; }
            catch { }
            return s;
        }*/

        public static bool fn_upload_file(string localname,string path, string n_name)
        {
            string tempfile = Environment.CurrentDirectory + "\\temp\\" + n_name;

            string name = Environment.CurrentDirectory + "\\images\\" + n_name;
            File.Copy(localname, name, true);

            File.Copy(localname, tempfile, true);

            string servername = fn_global.get_workdir(path) + "\\" + n_name;

            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    File.Copy(localname, servername, true);
                    return true;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    File.Copy(localname, servername, true);
                    return true;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    http FileOne = new http();

                    MyWsitMsg.MyWaitMSG msg = new MyWsitMsg.MyWaitMSG("Connecting...", inc.images_path + "\\" + "waiting.gif");
                    msg.MsgShow();

                    Thread.Sleep(200);

                    bool _r = FileOne.Upload_Request(System.Configuration.ConfigurationSettings.AppSettings["webserviceurl"] + "/upload.php"
                        , path
                        , tempfile);

                    try { msg.MsgClose(); }
                    catch { }

                    return _r;

                }
                catch
                {
                    //FileOne.Close();
                    return false;
                }
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];
                string ltapgs = System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                if (path != "")
                    path = "\\" + path;

                FTP ftp1 = new FTP(ftp_svr, ltapgs + path, user, pwd, int.Parse(port));

                try
                {
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {                        
                        ftp1.Put(tempfile);
                        ftp1.DisConnect();
                        return true;
                    }

                }
                catch (Exception e)
                {
                    ftp1.DisConnect();
                    string s = e.Message;
                }

                return false;
            }
        }

        public static bool fn_download_file(string localname, string path, string n_name)
        {
            string name = inc.images_path + "\\" + n_name;
            

            string servername = fn_global.get_workdir(path) + "\\" + n_name;
            if (!File.Exists(servername))
            {
                MessageBox.Show("Sever map file lost,please check");
                return false;
            }
            if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "1")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    File.Copy(servername, name, true);
                    return true;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "2")
            {
                if (!inc.remote_disk.Connected)
                    return false;
                else
                {
                    File.Copy(servername, name, true);
                    return true;
                }
            }
            else if (System.Configuration.ConfigurationSettings.AppSettings["UseRemoteDisk"] == "3")
            {
                try
                {
                    http FileOne = new http();

                    FileOne.FileLocalPath = name;
                    if (File.Exists(FileOne.FileLocalPath))
                        File.Delete(FileOne.FileLocalPath);

                    FileOne.Url = System.Configuration.ConfigurationSettings.AppSettings["webserviceurl"] + "/" + path + "/" + n_name;

                    FileOne.Connect();
                    if (FileOne.IsFileExist)
                    {
                        FileOne.DownloadFile();
                        FileOne.Close();
                        return true;
                    }
                    else
                    {
                        FileOne.Close();
                        return false;
                    }
                }
                catch
                {
                    //FileOne.Close();
                    return false;
                }
            }
            else
            {
                string ftp_svr = System.Configuration.ConfigurationSettings.AppSettings["FTPServer"];
                string ltapgs = System.Configuration.ConfigurationSettings.AppSettings["lta_pgs_forder"];
                string user = System.Configuration.ConfigurationSettings.AppSettings["FTPUser"];
                string pwd = System.Configuration.ConfigurationSettings.AppSettings["FTPPassword"];
                string port = System.Configuration.ConfigurationSettings.AppSettings["FTPPort"];

                if (path != "")
                    path = "\\" + path;

                FTP ftp1 = new FTP(ftp_svr, ltapgs + path, user, pwd, int.Parse(port));

                try
                {
                    ftp1.Connect();
                    if (ftp1.Connected)
                    {
                        string _path = Environment.CurrentDirectory + "\\" + path;
                        Tools.make_folder(_path);
                        // Thread.Sleep(500);
                        ftp1.Get(n_name, _path, n_name);
                        ftp1.DisConnect();
                        return true;
                    }
                }
                catch (Exception e)
                {
                    //string s = e.Message;
                    ftp1.DisConnect();
                    MessageBox.Show(e.Message);
                }

                return false;
            }
        }
    }
}
