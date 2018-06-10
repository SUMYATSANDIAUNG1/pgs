﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Data.Odbc;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Drawing.Printing;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

namespace LTA_PGS_20
{
    public partial class Form_Management : Form
    {
        #region var
        public int Page_num = 0;

        private int _current_vms_id = 0;
        private int Cur_Schedule_Group = -1;

        struct vms_msg_str
        {
            public int _cms_id;
            public Button _detail;
            public ComboBox _rows_data;
            public Label _rows_name;
            public int name_color;
            public int lots_color;
            public int mode;
            public int green;
            public int red;
        }
        vms_msg_str[] _vms_msg_str = null;

        TreeView treeView_cms = new TreeView();
        TreeView treeView_vms = new TreeView();
        TreeView treeView_msg = new TreeView();
        string current_combobox = "";

        bool check_pages1_change = false;//check user click "page 1" checkbox or not
        #endregion

        #region functions
        private int get_int_value(string str)
        {
            int r = 0;
            if (str != null)
            {
                try
                {
                    string temp = str.Substring(str.IndexOf("=") + 1);
                    r = int.Parse(temp.Trim());
                }
                catch { }
            }
            return r;
        }
        private string get_str_value(string str)
        {
            return Tools.get_str_value(str);
        }

        private void mouse_leave(object sender, EventArgs e)
        {
            ((TreeView)sender).Visible = false;
        }

        #endregion

        #region Main Control
        public Form_Management()
        {
            InitializeComponent();
            Initialize_schedule();
            Initialize_cms();
            Initialize_users();
            Initialize_premessage();
            Initialize_holiday();
            Initialize_area();
        }
        /// <summary>
        /// Tab select changed
        /// Case Tab
        /// 0 - library(premsg)
        /// 1 - VMS Config
        /// 2 - holiday config
        /// 3 - cms config
        /// 4 - users control
        /// 5 - adhoc message
        /// 6 - global parameter
        /// 7 - schedul
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl_management_SelectedIndexChanged(object sender, EventArgs e)
        {
            string _name = tabControl_management.TabPages[tabControl_management.SelectedIndex].Name;

            if (_name == "tabPage_library")
            {
                Library.Controls.Add(treeView_vms);
            }
            else if (_name == "tabPage_VMSconfig")
            {
                comboBox_VMSs.SelectedIndex = 0;
                tabPage_VMSconfig.Controls.Add(treeView_vms);
            }
            else if (_name == "tabPage_holiday")
            {
                load_holiday();
            }
            else if (_name == "tabPage_cms")
            {
                load_cms();
            }
            else if (_name == "tabPage_user")
            {
                fn_global.load_user_table();
                load_users_dataview();
            }
            else if (_name == "tabPage_global")
            {
                load_global_par();
            }
            else if (_name == "tabPage_schedule")
            {
                //load_adhoc_group();
            }
            else if (_name == "tabPage_area")
            {
                load_area();
            }
        }

        private void Form_Management_Load(object sender, EventArgs e)
        {
            _vms_msg_str = new vms_msg_str[18];

            Library.Controls.Add(treeView_vms);

                  
            comboBox_areas.Items.AddRange(inc.Area);
            comboBox_area.Items.AddRange(inc.Area);
            comboBox4.Items.AddRange(inc.Area);
            
            comboBox_VMSs.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox_vms_diming.Items.AddRange(inc.VMS_Diming_mode);

            comboBox_flash_mode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_flash_mode.Items.AddRange(inc.VMS_Flash_mode);

            int _hight = 48, _width = 190;
            for (int i = 0; i < 18; i++)
            {
                if (i % 3 == 0)
                {
                    _vms_msg_str[i]._rows_name = new Label();
                    _vms_msg_str[i]._rows_name.Top = 35 + (i / 3) * _hight;
                    _vms_msg_str[i]._rows_name.Left = 10;
                    _vms_msg_str[i]._rows_name.Text = "Row #" + ((i / 3) + 1).ToString();
                    _vms_msg_str[i]._rows_name.AutoSize = true;
                    groupBox5.Controls.Add(_vms_msg_str[i]._rows_name);
                }
                _vms_msg_str[i]._rows_data = new ComboBox();
                _vms_msg_str[i]._rows_data.Name = (i + 100).ToString();
                _vms_msg_str[i]._rows_data.Top = 35 + (i / 3) * _hight;
                _vms_msg_str[i]._rows_data.Left = 59 + (i % 3) * _width;
                _vms_msg_str[i]._rows_data.Items.AddRange(inc.carparkobject);
                _vms_msg_str[i]._rows_data.SelectedValueChanged += new EventHandler(cms_select_change);
                _vms_msg_str[i]._rows_data.DropDown += new EventHandler(cms_click);
                _vms_msg_str[i]._rows_data.DropDownHeight = 1;

                _vms_msg_str[i]._detail = new Button();
                _vms_msg_str[i]._detail.Name = i.ToString();
                _vms_msg_str[i]._detail.Text = "Detail";
                _vms_msg_str[i]._detail.Top = _vms_msg_str[i]._rows_data.Top - 1;
                _vms_msg_str[i]._detail.Left = _vms_msg_str[i]._rows_data.Left + _vms_msg_str[i]._rows_data.Width + 5;
                _vms_msg_str[i]._detail.Width = 45;
                _vms_msg_str[i]._detail.Click += new EventHandler(_show_VMS_detail);

                groupBox5.Controls.Add(_vms_msg_str[i]._rows_data);
                groupBox5.Controls.Add(_vms_msg_str[i]._detail);
                //Application.DoEvents();
            }

            init_global_par();

            load_library();
            Application.DoEvents();

            load_vmss_config();
            Application.DoEvents();

            init_schedule();

            //tabControl_management.SelectedIndex = Page_num;            
            int _i = tabControl_management.TabPages.Count - 1;
            while (_i >= 0)
            {
                bool _valid = false;
                for (int j = 0; j < inc.pagelist.Count; j++)
                {
                    if (inc.pagelist[j] == tabControl_management.TabPages[_i].Name)
                        _valid = true;
                    ;
                }
                if (!_valid)
                    tabControl_management.TabPages.Remove(tabControl_management.TabPages[_i]);

                _i -= 1;
            }

            fn_global.load_vms_treeview(treeView_vms, 0);

            for (int a = 0; a < tabControl_management.TabPages.Count; a++)
            {
                string _name = tabControl_management.TabPages[a].Name;
                if (inc.tabpagename == _name)
                {
                    tabControl_management.SelectedIndex = a;
                    break;
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Form_Priview f_priview = new Form_Priview();
            f_priview.ShowDialog();
        }

        private void _show_cms_config()
        {
            Form_Config_CMS f = new Form_Config_CMS();
            f.ShowDialog();
        }

        #endregion Main Control

        #region VMSs Table Control
        private void load_vmss_config()
        {
            comboBox_VMSs.Items.Clear();
            comboBox_VMSs.Items.AddRange(inc.VMS);
            comboBox_source_vms.Items.AddRange(inc.VMS);

            treeView_vms.Visible = false;
            treeView_vms.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(double_click_vmstree_tab_vms);
            treeView_vms.LostFocus += new EventHandler(mouse_leave);



            treeView_cms.Visible = false;
            treeView_cms.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(double_click_vmstree_cms);
            treeView_cms.LostFocus += new EventHandler(mouse_leave);
            groupBox5.Controls.Add(treeView_cms);

            clear_vms_config();
        }

        private void add_vms_ok()
        {
            fn_global.load_vmss_config();
            fn_global.load_vms_treeview(treeView_vms, 0);
            load_vmss_config();
        }

        private void button45_Click(object sender, EventArgs e)
        {
            Form_New_VMS f = new Form_New_VMS();
            f.MyEnvent += new CallBack_Confirm(add_vms_ok);

            f.ShowDialog();
        }

        private void double_click_vmstree_cms(object sender, TreeNodeMouseClickEventArgs e)
        {
            string parent_text = "", node_text = "";
            fn_global.Auto_close_treeview((TreeView)sender, ref parent_text, ref node_text);
            ((ComboBox)this.Controls.Find(current_combobox, true)[0]).Text = node_text;
        }

        private void double_click_vmstree_tab_vms(object sender, TreeNodeMouseClickEventArgs e)
        {
            string parent_text = "", node_text = "";
            fn_global.Auto_close_treeview((TreeView)sender, ref parent_text, ref node_text);
            comboBox_VMSs.Text = node_text;
        }

        private void comboBox_VMSs_Click(object sender, EventArgs e)
        {
            fn_global.show_treeview_location(treeView_vms, ((ComboBox)sender).Left, ((ComboBox)sender).Top + ((ComboBox)sender).Height);
        }
        private void cms_click(object sender, EventArgs e)
        {
            fn_global.show_treeview_location(treeView_cms, ((ComboBox)sender).Left, ((ComboBox)sender).Top + ((ComboBox)sender).Height);
            current_combobox = ((ComboBox)sender).Name;
        }
        private void cms_select_change(object sebder, EventArgs e)
        {
            int index = int.Parse(((ComboBox)sebder).Name) - 100;
            if (((ComboBox)sebder).SelectedIndex >= 0)
            {
                _vms_msg_str[index]._cms_id = inc.Carpark[((ComboBox)sebder).SelectedIndex].id;
                //_vms_msg_str[index].green = 0;
                //_vms_msg_str[index].lots_color = 0;
                //_vms_msg_str[index].mode = 0;
                //_vms_msg_str[index].name_color = 0;
                //_vms_msg_str[index].red = 0;
            }
        }
        private void clear_vms_config()
        {
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _vms_msg_str[(i * 3) + j]._rows_data.Text = "";
                }
            }
        }
        private void load_rows_config()
        {
            clear_vms_config();

            string _workdir = fn_global.get_workdir("gdm" + _current_vms_id.ToString());

            string _path = "gdm" + _current_vms_id.ToString();
            //this.Cursor = Cursors.Default;
            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < 6; i++)
            {
                Application.DoEvents();
                string file_name = "gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt";
                fn_global.fn_get_file(_path, file_name, file_name);
            }

            for (int i = 0; i < 6; i++)
            {
                Application.DoEvents();
                string gdm_file_name_line = _workdir + "\\gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt";
                //if (!File.Exists(gdm_file_name_line))
                //{
                //    continue;
                //}
                if (!fn_global.fn_file_exist(_path, "gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt"))
                    continue;

                MyFile f1 = new MyFile();
                f1.FileName = gdm_file_name_line;

                int toggle = get_int_value(f1.read_file_line());
                int timer = get_int_value(f1.read_file_line());
                int _lines = 0;
                for (int j = 0; j < 3; j++)
                {
                    _lines = (i * 3) + j;
                    _vms_msg_str[_lines]._cms_id = get_int_value(f1.read_file_line());
                    _vms_msg_str[_lines].name_color = get_int_value(f1.read_file_line());
                    _vms_msg_str[_lines].mode = get_int_value(f1.read_file_line());
                    _vms_msg_str[_lines].lots_color = get_int_value(f1.read_file_line());
                    _vms_msg_str[_lines].red = get_int_value(f1.read_file_line());
                    _vms_msg_str[_lines].green = get_int_value(f1.read_file_line());

                    int index_cms = fn_global.fn_cms_id2index(_vms_msg_str[_lines]._cms_id);
                    if (index_cms >= 0)
                        _vms_msg_str[_lines]._rows_data.Text = inc.Carpark[index_cms].name;
                }
                f1.close();
            }
            this.Cursor = Cursors.Default;
        }

        private void VMS_detail()
        {
            if (inc.VMS_Detail.update)
            {
                _vms_msg_str[inc.VMS_Detail.index].name_color = inc.VMS_Detail.name_color;
                _vms_msg_str[inc.VMS_Detail.index].lots_color = inc.VMS_Detail.lots_color;
                _vms_msg_str[inc.VMS_Detail.index].mode = inc.VMS_Detail.mode;
                _vms_msg_str[inc.VMS_Detail.index].green = inc.VMS_Detail.green;
                _vms_msg_str[inc.VMS_Detail.index].red = inc.VMS_Detail.red;
            }
        }

        private void _show_VMS_detail(object sebder, EventArgs e)
        {
            inc.VMS_Detail = new inc.vms_detail();
            inc.VMS_Detail.update = false;
            inc.VMS_Detail.index = int.Parse(((Button)sebder).Name);

            inc.VMS_Detail.name_color = _vms_msg_str[inc.VMS_Detail.index].name_color;
            inc.VMS_Detail.lots_color = _vms_msg_str[inc.VMS_Detail.index].lots_color;
            inc.VMS_Detail.mode = _vms_msg_str[inc.VMS_Detail.index].mode;
            inc.VMS_Detail.green = _vms_msg_str[inc.VMS_Detail.index].green;
            inc.VMS_Detail.red = _vms_msg_str[inc.VMS_Detail.index].red;

            Form_VMS_detail f = new Form_VMS_detail();
            f.MyEnvent += new CallBack_Detail(VMS_detail);
            f.ShowDialog();
        }

        private void load_vms_config_rows()
        {
            if (comboBox_VMSs.SelectedIndex >= 0)
            {
                comboBox_area.SelectedIndex = inc._vms[comboBox_VMSs.SelectedIndex].area - 1;
                fn_global.load_cms_treeview(treeView_cms, comboBox_area.SelectedIndex);
                textBox_id.Text = inc._vms[comboBox_VMSs.SelectedIndex]._id.ToString();
                _current_vms_id = int.Parse(textBox_id.Text);

                try { comboBox_vms_diming.SelectedIndex = inc._vms[comboBox_VMSs.SelectedIndex].dim; }
                catch { }
                try { comboBox_flash_mode.SelectedIndex = inc._vms[comboBox_VMSs.SelectedIndex].flash - 1; }
                catch { }
                textBox_location.Text = inc._vms[comboBox_VMSs.SelectedIndex].lcoation;
                textBox_timer.Text = inc._vms[comboBox_VMSs.SelectedIndex].timer.ToString();
                textBox_modify_name.Text = inc._vms[comboBox_VMSs.SelectedIndex].name;

                load_rows_config();
            }
        }
        private void comboBox_VMSs_SelectedIndexChanged(object sender, EventArgs e)
        {
            load_vms_config_rows();
        }

        private bool save_config()
        {
            try
            {
                //string gdm_file_name = _workdir + "\\gdm." + _current_vms_id.ToString() + ".txt";
                string _workdir = fn_global.get_workdir("gdm" + _current_vms_id.ToString());

                string gdm_file_name = _workdir + "\\gdm." + _current_vms_id.ToString() + ".txt";
                string gdm_file_name_line = "";
                Tools.make_folder(_workdir);


                //if (File.Exists(gdm_file_name))                
                //    File.Delete(gdm_file_name);

                if (fn_global.fn_file_exist(_workdir, "gdm." + _current_vms_id.ToString() + ".txt"))
                    fn_global.fn_file_delete(_workdir, "gdm." + _current_vms_id.ToString() + ".txt");

                MyFile f = new MyFile();
                f.FileName = gdm_file_name;
                f.write_to_file("gdm_id=" + _current_vms_id.ToString());

                inc._vms[comboBox_VMSs.SelectedIndex].flash = comboBox_flash_mode.SelectedIndex + 1;
                inc._vms[comboBox_VMSs.SelectedIndex].timer = int.Parse(textBox_timer.Text);

                for (int i = 0; i < 6; i++)
                {
                    f.write_to_file("gdm_row" + (i + 1).ToString() + "_cms_lta=1");
                    gdm_file_name_line = _workdir + "\\gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt";

                    //if (File.Exists(gdm_file_name_line))                    
                    //    File.Delete(gdm_file_name_line);       

                    if (fn_global.fn_file_exist(_workdir, "gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt"))
                        fn_global.fn_file_delete(_workdir, "gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt");

                    MyFile f1 = new MyFile();
                    f1.FileName = gdm_file_name_line;

                    f1.write_to_file("cp_toggle=" + inc._vms[comboBox_VMSs.SelectedIndex].flash.ToString());
                    f1.write_to_file("cp_timer=" + inc._vms[comboBox_VMSs.SelectedIndex].timer.ToString());
                    for (int j = 0; j < 3; j++)
                    {
                        int _lines = i * 3 + j;
                        int _index = _vms_msg_str[_lines]._rows_data.SelectedIndex;
                        string cp = (j + 1).ToString();
                        if (_index < 0)
                        {
                            f1.write_to_file("cp" + cp + "_cms_id=0");
                            f1.write_to_file("cp" + cp + "_name_color=1");
                            f1.write_to_file("cp" + cp + "_bar_mode=1");
                            f1.write_to_file("cp" + cp + "_lots_color=1");
                            f1.write_to_file("cp" + cp + "_red_percent=");
                            f1.write_to_file("cp" + cp + "_green_percent=");
                        }
                        else
                        {

                            f1.write_to_file("cp" + cp + "_cms_id=" + _vms_msg_str[_lines]._cms_id.ToString());
                            f1.write_to_file("cp" + cp + "_name_color=" + _vms_msg_str[_lines].name_color.ToString());
                            f1.write_to_file("cp" + cp + "_bar_mode=" + _vms_msg_str[_lines].mode.ToString());
                            f1.write_to_file("cp" + cp + "_lots_color=" + _vms_msg_str[_lines].lots_color.ToString());
                            if (_vms_msg_str[_lines].red > 0)
                                f1.write_to_file("cp" + cp + "_red_percent=" + _vms_msg_str[_lines].red.ToString());
                            else
                                f1.write_to_file("cp" + cp + "_red_percent=0");
                            if (_vms_msg_str[_lines].green > 0)
                                f1.write_to_file("cp" + cp + "_green_percent=" + _vms_msg_str[_lines].green.ToString());
                            else
                                f1.write_to_file("cp" + cp + "_green_percent=100");
                        }
                    }
                    f1.write_to_file("file_type=cms");
                    f1.close();
                    fn_global.fn_put_file("gdm" + _current_vms_id.ToString(), "gdm." + _current_vms_id.ToString() + "." + (i + 1).ToString() + ".txt");
                }
                f.close();
                fn_global.fn_put_file("gdm" + _current_vms_id.ToString(), "gdm." + _current_vms_id.ToString() + ".txt");

                return true;
            }
            catch { return false; }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string vms_name = "";
            if (MessageBox.Show("Save the VMS Config?", "VMS Config", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                vms_name = comboBox_VMSs.Text;
                if (_current_vms_id != 0)
                {
                    this.Cursor = Cursors.WaitCursor;

                    inc.db_control.SQL = "update tb_gdm set fd_name = '";
                    inc.db_control.SQL += textBox_modify_name.Text;
                    inc.db_control.SQL += "' , fd_location = '";
                    inc.db_control.SQL += textBox_location.Text;
                    inc.db_control.SQL += "' , fd_dim = '";
                    inc.db_control.SQL += comboBox_vms_diming.SelectedIndex.ToString();
                    inc.db_control.SQL += "' , fd_flash = '";
                    inc.db_control.SQL += (comboBox_flash_mode.SelectedIndex + 1).ToString();
                    inc.db_control.SQL += "' , fd_timer= '";
                    inc.db_control.SQL += textBox_timer.Text;
                    inc.db_control.SQL += "' , fd_area= '";
                    inc.db_control.SQL += comboBox_area.SelectedIndex + 1;
                    inc.db_control.SQL += "' ";
                    inc.db_control.SQL += "where fd_id = '";
                    inc.db_control.SQL += _current_vms_id.ToString() + "'";

                    try
                    {
                        if (inc.db_control.SQLExecuteReader() != null)
                        {
                            if (save_config())
                            {

                                string _name = "Update VMS Config ";
                                string _title = "Update VMS " + _current_vms_id.ToString() + " Config";
                                string _path = "";
                                string _file = "update.txt";
                                string _cmd = "<update><gdm=" + textBox_id.Text + ">";
                                send_cmd(_cmd, _path, _file, _name, _title);

                                //MessageBox.Show("Save VMS Config Successful");
                                fn_global.log_operateion((int)inc.LOGMSGCODE.VMU01, comboBox_VMSs.Text, 1);

                                fn_global.load_vmss_config();
                                load_vmss_config();
                            }
                            else
                                MessageBox.Show("Save VMS Config Files Failed");
                        }
                        else
                            MessageBox.Show("Save VMS Config Failed");
                        //load_vmss_config();
                        //comboBox_VMSs.SelectedIndex = 0;
                        //load_rows_config();
                        comboBox_VMSs.Text = vms_name;
                    }
                    catch
                    {
                        MessageBox.Show("Save VMS Config Failed");
                    }

                    this.Cursor = Cursors.Default;
                }
            }
            else
            {

            }

        }

        private void button46_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete the VMS?", "Delete VMS", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                inc.db_control.SQL = "update tb_gdm set fd_delete = 1 where fd_id='" + _current_vms_id.ToString() + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.VMD01, comboBox_VMSs.Text, 1);
                    fn_global.load_vmss_config();
                    load_vmss_config();
                    fn_global.load_vms_treeview(treeView_vms, 0);
                }
            }
        }

        private void button33_Click(object sender, EventArgs e)
        {
            if (textBox_id.Text != "")
            {
                Form_Dimming f1 = new Form_Dimming();
                f1.VMSid = int.Parse(textBox_id.Text);
                f1.fn_type = 2;
                f1.ShowDialog();
            }

        }

        private void send_cmd_dim(string cmd, string path, string file)
        {
            string _str = "";

            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] != "1")
                fn_global.fn_file_delete(path, file);

            if (fn_global.fn_tcp_send(cmd))
            {
                if (fn_global.fn_cmd_check_finish(path, file, ref _str))
                {
                    Form_Dimming f1 = new Form_Dimming();
                    f1.VMSid = int.Parse(textBox_id.Text);
                    f1.fn_type = 0;
                    f1.str = _str;
                    f1.ShowDialog();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //string _name = "VMS Update Dimming ";
            //string _title = "VMS " + textBox_id.Text + " Update Dimming ";
            string _path = "gdm" + textBox_id.Text;

            string _cmd = "<update_dim><gdm=" + textBox_id.Text + ">";
            string _file = "dimupdate.txt";
            //send_cmd_dim(_cmd, _path, _file);
            string _name = "VMS Dimming Update ";
            string _title = "VMS " + textBox_id.Text + "  Dimming Update";
            send_cmd(_cmd, _path, _file, _name, _title);
        }

        private void button36_Click(object sender, EventArgs e)
        {
            //string _name = "VMS Get Dimming ";
            //string _title = "VMS " + textBox_id.Text + " Get Dimming ";
            string _path = "gdm" + textBox_id.Text;

            string _cmd = "<get_dim><gdm=" + textBox_id.Text + ">";
            string _file = "dimget.txt";
            send_cmd_dim(_cmd, _path, _file);

        }

        private void send_cmd(string cmd, string path, string file, string formname, string title)
        {
            string _str = "";
            this.Cursor = Cursors.WaitCursor;

            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] != "1")
                fn_global.fn_file_delete(path, file);

            if (fn_global.fn_tcp_send(cmd))
            {
                if (fn_global.fn_cmd_check_finish(path, file, ref _str))
                {
                    this.Cursor = Cursors.Default;
                    int _mode = 0;
                    if (file.IndexOf("update") >= 0)
                        _mode = 1;
                    fn_global.fn_return_status(_str, formname, title, _mode);
                }
            }
            this.Cursor = Cursors.Default;
        }

        private void button34_Click(object sender, EventArgs e)
        {
            /*"<cms_status>"
                "<led_status>"
             * "<get_param>"
             * <adhoc>
             * <update_all>
             * <update_adhoc>
             * <reload server>
             * */
            string _name = "VMS Detect Pixel ";
            string _title = "VMS " + textBox_id.Text + " Detect Pixel ";
            string _path = "gdm" + textBox_id.Text;
            string _file = "pixel.txt";
            string _cmd = "<get_pixel><gdm=" + textBox_id.Text + ">";
            send_cmd(_cmd, _path, _file, _name, _title);
        }

        private void button35_Click(object sender, EventArgs e)
        {
            string _name = "VMS Get Pixel ";
            string _title = "VMS " + textBox_id.Text + " Get Pixel ";
            string _path = "gdm" + textBox_id.Text;
            string _file = "pixel.txt";
            string _cmd = "<get_pixel><gdm=" + textBox_id.Text + ">";
            send_cmd(_cmd, _path, _file, _name, _title);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string _name = "VMS Status";
            string _title = "VMS " + textBox_id.Text + " Status";
            string _path = "gdm" + textBox_id.Text;
            string _file = "gdmstatus.txt";
            string _cmd = "<get_status><gdm=" + textBox_id.Text + ">";
            send_cmd(_cmd, _path, _file, _name, _title);
        }

        private void button44_Click(object sender, EventArgs e)
        {
            string _name = "VMS Firmware Version";
            string _title = "VMS " + textBox_id.Text + " Firmware Version";
            string _path = "gdm" + textBox_id.Text;
            string _file = "ledstatus.txt";
            string _cmd = "<get_version><gdm=" + textBox_id.Text + ">";

            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] == "1")
            {
                _file = "ledstatus2.txt";
            }

            send_cmd(_cmd, _path, _file, _name, _title);
        }

        private void button47_Click(object sender, EventArgs e)
        {
            string _name = "VMS Firmware Version";
            string _title = "VMS " + textBox_id.Text + " Firmware Version";
            string _file = "update.txt";
            string _path = "gdm" + textBox_id.Text;

            string _cmd = "<update_cms>";
            send_cmd(_cmd, _path, _file, _name, _title);
        }
        #endregion

        #region Premessage Table Control
        private void Initialize_premessage()
        {
            dataGridView_premsg.Columns.Add("Id", "ID");
            dataGridView_premsg.Columns.Add("gdm", "VMS ID");
            dataGridView_premsg.Columns.Add("name", "Name");
            dataGridView_premsg.Columns.Add("l1", "Row 1");
            dataGridView_premsg.Columns.Add("l2", "Row 2");
            dataGridView_premsg.Columns.Add("l3", "Row 3");
            dataGridView_premsg.Columns.Add("l4", "Row 4");
            dataGridView_premsg.Columns.Add("l5", "Row 5");
            dataGridView_premsg.Columns.Add("l6", "Row 6");
            dataGridView_premsg.Columns.Add("new_update", "0");//0-no modify 1-modify 2-new

            dataGridView_premsg.Columns["name"].Width = 190;
            dataGridView_premsg.Columns["l1"].Width = 150;
            dataGridView_premsg.Columns["l2"].Width = 150;
            dataGridView_premsg.Columns["l3"].Width = 150;
            dataGridView_premsg.Columns["l4"].Width = 150;
            dataGridView_premsg.Columns["l5"].Width = 150;
            dataGridView_premsg.Columns["l6"].Width = 150;
            dataGridView_premsg.Columns["new_update"].Visible = false;
        }
        private void load_library()
        {
            comboBox_vms.Items.AddRange(inc.VMS);

            inc.db_control.SQL = "select * from tb_premsg order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            dataGridView_premsg.Rows.Clear();
            int i = 0;
            while (MyReader.Read())
            {
                dataGridView_premsg.Rows.Add();
                dataGridView_premsg.Rows[i].Cells["Id"].Value = (Int64)MyReader["fd_id"];
                dataGridView_premsg.Rows[i].Cells["gdm"].Value = pre_gdmid2name((Int32)MyReader["fd_gdm"]);
                dataGridView_premsg.Rows[i].Cells["name"].Value = (String)MyReader["fd_name"];

                for (int j = 1; j <= 6; j++)
                {
                    string _cells = "l" + j.ToString();
                    string _field = "fd_line" + j.ToString();
                    dataGridView_premsg.Rows[i].Cells[_cells].Value = MyReader[_field] != DBNull.Value ? (String)MyReader[_field] : "";
                }
                i++;
            }
            dataGridView_premsg.EndEdit();

            treeView_vms.Visible = false;
            treeView_vms.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(double_click_vmstree);
            treeView_vms.LostFocus += new EventHandler(mouse_leave);

            update_info_groupbox();
        }

        private void double_click_vmstree(object sender, TreeNodeMouseClickEventArgs e)
        {
            string parent_text = "", node_text = "";
            fn_global.Auto_close_treeview((TreeView)sender, ref parent_text, ref node_text);
            comboBox_vms.Text = node_text;
        }

        private void comboBox_vms_Click(object sender, EventArgs e)
        {

        }

        private int pre_gdmname2id(object name)
        {
            string _gdm_name = Tools.get_value(name);
            if (_gdm_name == "All")
                return 0;
            else
            {
                int index = fn_global.fn_vms_name2index(_gdm_name);
                if (index >= 0)
                    return inc._vms[index]._id;
                else
                {
                    try { return ((int)name); }
                    catch { return 0; }
                }
            }
        }

        private string pre_gdmid2name(int id)
        {
            if (id == 0)
                return "All";
            else
            {
                int _index = fn_global.fn_vms_id2index(id);
                if (_index >= 0)
                    return inc._vms[_index].name;
                else
                    return id.ToString();
            }

        }

        //private void button5_Click(object sender, EventArgs e)
        //{
        //    for (int i = 0; i < dataGridView_premsg.Rows.Count; i++)
        //    {
        //        string _update = Tools.get_value(dataGridView_premsg.Rows[i].Cells["new_update"].Value);
        //        string _id = Tools.get_value(dataGridView_premsg.Rows[i].Cells["Id"].Value);
        //        int _gdmid = pre_gdmname2id(dataGridView_premsg.Rows[i].Cells["gdm"].Value);

        //        string _name = Tools.get_value(dataGridView_premsg.Rows[i].Cells["name"].Value);
        //        string _l1 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l1"].Value);
        //        string _l2 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l2"].Value);
        //        string _l3 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l3"].Value);
        //        string _l4 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l4"].Value);
        //        string _l5 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l5"].Value);
        //        string _l6 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l6"].Value);
        //        if (_update == "1")
        //        {
        //            inc.db_control.SQL = inc.db_control.udpate_table("tb_premsg", _id, _gdmid.ToString(), "", _name, _l1, _l2, _l3, _l4, _l5, _l6);
        //            inc.db_control.SQL += " where fd_id='" + _id + "'";
        //        }
        //        if (_update == "2")
        //        {
        //            inc.db_control.SQL = inc.db_control.insert_table("tb_premsg", "", _gdmid.ToString(), "", _name, _l1, _l2, _l3, _l4, _l5, _l6);
        //        }

        //        inc.db_control.SQLExecuteReader();
        //    }
        //    inc.tcp_client.SendBuffer("<update_premsg>");
        //}

        private void update_info_groupbox()
        {
            int i = -1;
            if (dataGridView_premsg.CurrentRow != null)
            {
                i = dataGridView_premsg.CurrentRow.Index;
            }

            if (i < 0)
                return;


            int _index = fn_global.fn_vms_id2index(pre_gdmname2id(dataGridView_premsg.Rows[i].Cells["gdm"].Value));
            if (_index >= 0)
                comboBox_vms.SelectedIndex = _index;
            else
                comboBox_vms.SelectedIndex = -1;

            textBox_msgid.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["Id"].Value);
            textBox_msgname.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["name"].Value);
            textBox_line1.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l1"].Value);
            textBox_line2.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l2"].Value);
            textBox_line3.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l3"].Value);
            textBox5.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l4"].Value);
            textBox3.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l5"].Value);
            textBox2.Text = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l6"].Value);
        }

        private void dataGridView_premsg_SelectionChanged(object sender, EventArgs e)
        {
            update_info_groupbox();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string _gdm = "0";
            int index = fn_global.fn_vms_name2index(comboBox_vms.Text);
            if (index >= 0)
                _gdm = inc._vms[index]._id.ToString();

            inc.db_control.SQL = "insert into tb_premsg (fd_gdm,fd_mode,fd_timer,fd_color,fd_name,fd_line1,fd_line2,fd_line3,fd_line4,fd_line5,fd_line6)";
            inc.db_control.SQL += "values (" + _gdm + ",1,2,1,'" + textBox_msgname.Text + "','" + textBox_line1.Text + "','" + textBox_line2.Text + "','"
                + textBox_line3.Text + "','" + textBox5.Text + "','" + textBox3.Text + "','" + textBox2.Text + "')";
            //inc.db_control.SQL = inc.db_control.insert_table("tb_premsg", "", _gdm, "1","2","1", textBox_msgname.Text, textBox_line1.Text, textBox_line2.Text, textBox_line3.Text, textBox5.Text, textBox3.Text, textBox2.Text);
            if (inc.db_control.SQLExecuteReader() != null)
            {
                fn_global.log_operateion((int)inc.LOGMSGCODE.PMI01, textBox_msgname.Text, 1);
                load_library();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_vms.SelectedIndex >= 0)
                    dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["gdm"].Value = comboBox_vms.Text;
                else
                    dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["gdm"].Value = "0";

                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["name"].Value = textBox_msgname.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l1"].Value = textBox_line1.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l2"].Value = textBox_line2.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l3"].Value = textBox_line3.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l4"].Value = textBox5.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l5"].Value = textBox3.Text;
                dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["l6"].Value = textBox2.Text;
                string _update = Tools.get_value(dataGridView_premsg.Rows[dataGridView_premsg.Rows.Count - 1].Cells["new_update"].Value);
                if (_update != "2")
                    dataGridView_premsg.Rows[dataGridView_premsg.CurrentRow.Index].Cells["new_update"].Value = "1";
                dataGridView_premsg.EndEdit();

                int i = dataGridView_premsg.CurrentRow.Index;
                _update = Tools.get_value(dataGridView_premsg.Rows[i].Cells["new_update"].Value);
                string _id = Tools.get_value(dataGridView_premsg.Rows[i].Cells["Id"].Value);

                string _gdm_name = Tools.get_value(dataGridView_premsg.Rows[i].Cells["gdm"].Value);
                string _gdm = _gdm_name;
                int index = fn_global.fn_vms_name2index(_gdm_name);
                if (index >= 0)
                    _gdm = inc._vms[index]._id.ToString();

                string _name = Tools.get_value(dataGridView_premsg.Rows[i].Cells["name"].Value);
                string _l1 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l1"].Value);
                string _l2 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l2"].Value);
                string _l3 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l3"].Value);
                string _l4 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l4"].Value);
                string _l5 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l5"].Value);
                string _l6 = Tools.get_value(dataGridView_premsg.Rows[i].Cells["l6"].Value);

                inc.db_control.SQL = inc.db_control.udpate_table("tb_premsg", _id, _gdm, "0", "4", "1", _name, _l1, _l2, _l3, _l4, _l5, _l6);
                inc.db_control.SQL += " where fd_id='" + _id + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    MessageBox.Show("Update message library successful");
                    fn_global.log_operateion((int)inc.LOGMSGCODE.PMU01, _name, 1);
                    load_library();
                }
                else
                    MessageBox.Show("Update message library failed");
            }
            catch (Exception e1)
            {
                MessageBox.Show("Update message library failed");
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete the Message?", "Message Library", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int i = dataGridView_premsg.CurrentRow.Index;
                string _id = Tools.get_value(dataGridView_premsg.Rows[i].Cells["Id"].Value);

                inc.db_control.SQL = inc.db_control.delete_table("tb_premsg");
                inc.db_control.SQL += " where fd_id='" + _id + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.PMD01, Tools.get_value(dataGridView_premsg.Rows[i].Cells["name"].Value), 1);
                    dataGridView_premsg.Rows.Remove(dataGridView_premsg.CurrentRow);
                }
            }
        }

        private void comboBox_vms_DropDown(object sender, EventArgs e)
        {
            fn_global.show_treeview_location(treeView_vms, ((ComboBox)sender).Left, ((ComboBox)sender).Top + ((ComboBox)sender).Height);
        }

        #endregion

        #region holidays Table Control
        private void Initialize_holiday()
        {
            dataGridView_holiday.Columns.Add("Id", "ID");
            dataGridView_holiday.Columns.Add("name", "Name");
            dataGridView_holiday.Columns.Add("date", "Date");

            dataGridView_holiday.Columns["name"].Width = 290;
            dataGridView_holiday.Columns["date"].Width = 190;
            dataGridView_holiday.Columns["Id"].Visible = false;


        }
        /// <summary>
        /// load holidays
        /// </summary>
        private void load_holiday()
        {
            inc.db_control.SQL = "select fd_id,fd_name,fd_year,fd_month,fd_day from tb_holiday order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            dataGridView_holiday.Rows.Clear();

            int i = 0;
            while (MyReader.Read())
            {
                dataGridView_holiday.Rows.Add();
                dataGridView_holiday.Rows[i].Cells["Id"].Value = MyReader.GetInt32(0).ToString();
                dataGridView_holiday.Rows[i].Cells["name"].Value = MyReader.GetString(1);
                dataGridView_holiday.Rows[i].Cells["date"].Value = MyReader.GetString(4) + "/" + MyReader.GetInt32(3) + "/" + MyReader.GetInt32(2);
                i++;
            }
            dataGridView_premsg.EndEdit();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            try
            {

                dataGridView_holiday.Rows.Add();
                int i = dataGridView_holiday.Rows.Count - 1;
                dataGridView_holiday.Rows[i].Cells["Id"].Value = "";
                dataGridView_holiday.Rows[i].Cells["name"].Value = textBox_holiday_name.Text;
                dataGridView_holiday.Rows[i].Cells["date"].Value = dateTimePicker_holiday.Value.ToString("dd/MM/yyyy");

                string _name = Tools.get_value(dataGridView_holiday.Rows[i].Cells["name"].Value);
                string _dd = dateTimePicker_holiday.Value.Date.ToString("dd");
                string _MM = dateTimePicker_holiday.Value.Date.ToString("MM");
                string _yyyy = dateTimePicker_holiday.Value.Date.ToString("yyyy");

                inc.db_control.SQL = inc.db_control.insert_table("tb_holiday", "", _name, _yyyy, _MM, _dd, "");
                if (inc.db_control.SQLExecuteReader() != null)
                    fn_global.log_operateion((int)inc.LOGMSGCODE.HDI01, _name, 1);
                else
                    MessageBox.Show("Add Holiday Failed");
            }
            catch
            {
                MessageBox.Show("Add Holiday Failed");
            }
        }

        private void button22_Click(object sender, EventArgs e)
        {
            try
            {
                int i = dataGridView_holiday.CurrentRow.Index;
                dataGridView_holiday.Rows[i].Cells["name"].Value = textBox_holiday_name.Text;
                dataGridView_holiday.Rows[i].Cells["date"].Value = dateTimePicker_holiday.Value.ToString("dd/MM/yyyy");

                string _name = Tools.get_value(dataGridView_holiday.Rows[i].Cells["name"].Value);
                string _dd = dateTimePicker_holiday.Value.Date.ToString("dd");
                string _MM = dateTimePicker_holiday.Value.Date.ToString("MM");
                string _yyyy = dateTimePicker_holiday.Value.Date.ToString("yyyy");
                string _id = dataGridView_holiday.Rows[i].Cells["Id"].Value.ToString();

                inc.db_control.SQL = inc.db_control.udpate_table("tb_holiday", _id, _name, _yyyy, _MM, _dd);
                inc.db_control.SQL += " where fd_id='" + _id + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.HDU01, _name, 1);
                }
                else
                    MessageBox.Show("Save Holiday Failed");
            }
            catch
            {
                MessageBox.Show("Save Holiday Failed");
            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete the Holiday?", "Delete Holiday", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int i = dataGridView_holiday.CurrentRow.Index;

                    inc.db_control.SQL = inc.db_control.delete_table("tb_holiday");
                    inc.db_control.SQL += " where fd_id='" + Tools.get_value(dataGridView_holiday.Rows[i].Cells["Id"].Value) + "'";
                    if (inc.db_control.SQLExecuteReader() != null)
                    {
                        fn_global.log_operateion((int)inc.LOGMSGCODE.HDD01, Tools.get_value(dataGridView_holiday.Rows[i].Cells["name"].Value), 1);
                        dataGridView_holiday.Rows.Remove(dataGridView_holiday.CurrentRow);
                    }
                    else
                    {
                        MessageBox.Show("Delete Holiday Failed");
                    }
                }
                catch { MessageBox.Show("Delete Holiday Failed"); }
            }
        }

        private void dataGridView_holiday_SelectionChanged(object sender, EventArgs e)
        {
            int i = dataGridView_holiday.CurrentRow.Index;
            textBox_holiday_name.Text = Tools.get_value(dataGridView_holiday.Rows[i].Cells["name"].Value);

            DateTimeFormatInfo dtFI = new DateTimeFormatInfo();
            dtFI.ShortDatePattern = "dd/MM/yyyy";
            string _time = Tools.get_value(dataGridView_holiday.Rows[i].Cells["date"].Value);
            if (_time != "")
                dateTimePicker_holiday.Value = DateTime.Parse(_time, dtFI);
        }
        #endregion

        #region Carpark Table Control
        private void Initialize_cms()
        {
            dataGridView_cms.Columns.Add("Id", "ID");
            dataGridView_cms.Columns.Add("ip", "IP");
            dataGridView_cms.Columns.Add("name", "Name");
            dataGridView_cms.Columns.Add("display", "Display Name");
            dataGridView_cms.Columns.Add("limit", "Limit");

            dataGridView_cms.Columns["ip"].Visible = false;
            dataGridView_cms.Columns["limit"].Visible = false;
            dataGridView_cms.Columns["id"].Width = 150;
            dataGridView_cms.Columns["Name"].Width = 300;
            dataGridView_cms.Columns["display"].Width = 300;
        }
        private bool fn_check_cms()
        {
            for (int i = 0; i < dataGridView_cms.Rows.Count; i++)
            {
                if (dataGridView_cms.Rows[i].Cells["display"].Value.ToString() == textBox_cms_display.Text)
                {
                    MessageBox.Show("The CMS name already exist");
                    return false;
                }
            }
            return true;
        }

        private void load_cms()
        {
            dataGridView_cms.Rows.Clear();
            for (int i = 0; i < inc.Carpark.Length; i++)
            {
                dataGridView_cms.Rows.Add();
                dataGridView_cms.Rows[i].Cells["id"].Value = inc.Carpark[i].id.ToString();
                dataGridView_cms.Rows[i].Cells["ip"].Value = inc.Carpark[i].ip;
                dataGridView_cms.Rows[i].Cells["limit"].Value = inc.Carpark[i].limit;
                dataGridView_cms.Rows[i].Cells["name"].Value = inc.Carpark[i].name2;
                dataGridView_cms.Rows[i].Cells["display"].Value = inc.Carpark[i].name;
            }
            dataGridView_cms.EndEdit();

            if (dataGridView_cms.CurrentRow != null)
            {
                int i = dataGridView_cms.CurrentRow.Index;
                textBox_cms_id.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["Id"].Value);
                textBox_cms_ip.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["ip"].Value);
                textBox_limit.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["limit"].Value);
                textBox_cms_name.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["name"].Value);
                textBox_cms_display.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["display"].Value);
                int idx = fn_global.fn_cms_id2index(int.Parse(textBox_cms_id.Text));
                comboBox4.SelectedIndex = fn_global.fn_area_id2index(inc.Carpark[idx].area);
                textBox20.Text = inc.Carpark[idx].latitude;
                textBox16.Text = inc.Carpark[idx].longtitude;
            }

        }
        private void dataGridView3_SelectionChanged(object sender, EventArgs e)
        {
            int i = dataGridView_cms.CurrentRow.Index;
            try
            {
                if (textBox_cms_id.Text == "") return;

                textBox_cms_id.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["Id"].Value);
                textBox_cms_ip.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["ip"].Value);
                textBox_limit.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["limit"].Value);
                textBox_cms_name.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["name"].Value);
                textBox_cms_display.Text = Tools.get_value(dataGridView_cms.Rows[i].Cells["display"].Value);

                int idx = fn_global.fn_cms_id2index(int.Parse(textBox_cms_id.Text));
                comboBox4.SelectedIndex = fn_global.fn_area_id2index(inc.Carpark[idx].area);
                textBox20.Text = inc.Carpark[idx].latitude;
                textBox16.Text = inc.Carpark[idx].longtitude;
            }
            catch { }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            if (!fn_check_cms())
                return;
            try
            {
                dataGridView_cms.Rows.Add();
                int i = dataGridView_cms.Rows.Count - 1;
                dataGridView_cms.Rows[i].Cells["Id"].Value = textBox_cms_id.Text;
                dataGridView_cms.Rows[i].Cells["ip"].Value = textBox_cms_ip.Text;
                dataGridView_cms.Rows[i].Cells["limit"].Value = textBox_limit.Text;
                dataGridView_cms.Rows[i].Cells["name"].Value = textBox_cms_name.Text;
                dataGridView_cms.Rows[i].Cells["display"].Value = textBox_cms_display.Text;

                int areaidx = fn_global.fn_area_name2index(comboBox4.Text);

                inc.db_control.SQL = inc.db_control.insert_table("tb_carpark", textBox_cms_id.Text
                   , textBox_cms_display.Text, textBox_cms_name.Text, textBox_cms_ip.Text
                    , textBox_limit.Text, "0", "0", "0", "0", "0", "08:00", "23:00", inc.tb_area[areaidx].areaid.ToString(), "0", textBox20.Text, textBox16.Text);
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.CPI01, textBox_cms_name.Text, 1);
                    //inc.db_control.SQL = "update tb_carpark set fd_id="+id+" where fd_id="

                    MessageBox.Show("Add CMS config Successful");
                }
                else
                    MessageBox.Show("Add CMS config Failed");
            }
            catch { MessageBox.Show("Add CMS config Failed"); }
            fn_global.load_cmss_config();
            load_cms();
        }

        private void button23_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete the CMS?", "Delete CMS", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int i = dataGridView_cms.CurrentRow.Index;

                    inc.db_control.SQL = inc.db_control.delete_table("tb_carpark");
                    inc.db_control.SQL += " where fd_id='" + Tools.get_value(dataGridView_cms.Rows[i].Cells["id"].Value) + "'";
                    if (inc.db_control.SQLExecuteReader() != null)
                    {
                        fn_global.log_operateion((int)inc.LOGMSGCODE.CPD01, Tools.get_value(dataGridView_cms.Rows[i].Cells["name"].Value), 1);

                        dataGridView_cms.Rows.Remove(dataGridView_cms.CurrentRow);
                        fn_global.load_cmss_config();
                        load_cms();
                    }
                    else
                        MessageBox.Show("Delete CMS Failed");

                }
                catch { MessageBox.Show("Delete CMS Failed"); }
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            try
            {
                int i = dataGridView_cms.CurrentRow.Index;
                dataGridView_cms.Rows[i].Cells["Id"].Value = textBox_cms_id.Text;
                dataGridView_cms.Rows[i].Cells["ip"].Value = textBox_cms_ip.Text;
                dataGridView_cms.Rows[i].Cells["limit"].Value = textBox_limit.Text;
                dataGridView_cms.Rows[i].Cells["name"].Value = textBox_cms_name.Text;
                dataGridView_cms.Rows[i].Cells["display"].Value = textBox_cms_display.Text;

                int index = fn_global.fn_cms_id2index(int.Parse(textBox_cms_id.Text));

                inc.db_control.SQL = inc.db_control.udpate_table("tb_carpark", textBox_cms_id.Text
                    , textBox_cms_display.Text, textBox_cms_name.Text, textBox_cms_ip.Text
                    , textBox_limit.Text, inc.Carpark[index].map_point.X.ToString(), inc.Carpark[index].map_point.Y.ToString()
                    , inc.Carpark[index].map_id.ToString(), inc.Carpark[index].x_out.ToString(), inc.Carpark[index].y_out.ToString()
                    , inc.Carpark[index].f_time, inc.Carpark[index].t_time, inc.tb_area[comboBox4.SelectedIndex].areaid.ToString()
                    , "0", textBox20.Text, textBox16.Text);
                inc.db_control.SQL += " where fd_id='" + dataGridView_cms.Rows[i].Cells["id"].Value.ToString() + "'";
                inc.db_control.SQLExecuteReader();
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.CPU01, textBox_cms_name.Text, 1);
                    MessageBox.Show("Update CMS config Successful");
                }
                else
                    MessageBox.Show("Update CMS config Failed");
            }
            catch
            {
                MessageBox.Show("Update CMS config Failed");
            }
            fn_global.load_cmss_config();
            load_cms();
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Form_Config_CMS f = new Form_Config_CMS();
            f.CMS_id = int.Parse(textBox_cms_id.Text);
            f.ShowDialog();
        }

        #endregion

        #region User Control
        private void Initialize_users()
        {
            dataGridView_users.Columns.Add("Id", "ID");
            dataGridView_users.Columns.Add("name", "Name");
            dataGridView_users.Columns.Add("group", "Group");
            dataGridView_users.Columns.Add("pwd", "Group");
            dataGridView_users.Columns.Add("times", "Last Time");
            dataGridView_users.Columns.Add("lasttime", "Last Time");

            dataGridView_users.Columns["Id"].Visible = false;
            dataGridView_users.Columns["pwd"].Visible = false;
            dataGridView_users.Columns["times"].Visible = false;
            dataGridView_users.Columns["name"].Width = 200;
            dataGridView_users.Columns["group"].Width = 300;
            dataGridView_users.Columns["lasttime"].Width = 200;
        }
        private void load_users_dataview()
        {
            comboBox_user_group.Items.AddRange(inc.User_Group);
            dataGridView_users.Rows.Clear();

            for (int i = 0; i < inc.TB_user.Length; i++)
            {
                int user_index = fn_global.fn_user_id2index(inc.currentuser);

                //if ((inc.TB_user[user_index].level > inc.TB_user[i].level)||(inc.currentuser == inc.TB_user[i].id))
                //{
                dataGridView_users.Rows.Add();
                int _c = dataGridView_users.Rows.Count - 1;

                dataGridView_users.Rows[_c].Cells["Id"].Value = inc.TB_user[i].id.ToString();
                dataGridView_users.Rows[_c].Cells["name"].Value = inc.TB_user[i].name;
                dataGridView_users.Rows[_c].Cells["group"].Value = inc.User_Group[inc.TB_user[i].level - 1].ToString();
                dataGridView_users.Rows[_c].Cells["pwd"].Value = inc.TB_user[i].pwd;
                dataGridView_users.Rows[_c].Cells["times"].Value = inc.TB_user[i].times.ToString();
                if (inc.TB_user[i].lasttime >= Tools.str_to_datetime("2000-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss"))
                    dataGridView_users.Rows[_c].Cells["lasttime"].Value = inc.TB_user[i].lasttime.ToString("dd/MM/yyyy HH:mm:ss");
                else
                    dataGridView_users.Rows[_c].Cells["lasttime"].Value = "";
                //}
            }
            dataGridView_users.EndEdit();

            groupBox17.Enabled = true;
            comboBox1.Items.Clear();
            int _c_level = inc.TB_user[fn_global.fn_user_id2index(inc.currentuser)].level;
            if (_c_level == 3)
            {
                comboBox1.Items.Add("Supervisor");
                comboBox1.Items.Add("Operator");
            }
            else if (_c_level == 2)
            {
                comboBox1.Items.Add("Operator");
            }
            else
            {
                groupBox17.Enabled = false;
            }

            try
            {
                int i = 0;
                textBox_user_id.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["Id"].Value);
                textBox_user_name.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["name"].Value);
                int _index = 0;
                for (int j = 0; j < inc.User_Group.Length; j++)
                {
                    string _s = Tools.get_value(dataGridView_users.Rows[i].Cells["group"].Value);
                    if (_s == inc.User_Group[j].ToString())
                    {
                        _index = j;
                        break;
                    }
                }
                comboBox_user_group.SelectedIndex = _index;
                textBox_user_pwd.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["pwd"].Value);
                textBox_user_times.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["times"].Value);
            }
            catch { }
        }
        private bool check_user_add()
        {
            if (textBox_user_pwd.Text.Length < 8)
            {
                MessageBox.Show("At least 8 Charaters");
                return false;
            }

            for (int i = 0; i < inc.TB_user.Length; i++)
            {
                //if (inc.TB_user[i].id.ToString() == textBox_user_id.Text)
                //{
                //    MessageBox.Show("The user ID already exist");
                //    return false;
                //}

                if (inc.TB_user[i].name == textBox_user_name.Text)
                {
                    MessageBox.Show("The user name already exist");
                    return false;
                }
            }
           
            return true;
        }
        private void button31_Click(object sender, EventArgs e)
        {
            if (!check_user_add())
                return;
            try
            {
                dataGridView_users.Rows.Add();
                textBox_user_times.Text = "0";
                int i = dataGridView_users.Rows.Count - 1;
                dataGridView_users.Rows[i].Cells["Id"].Value = "";
                dataGridView_users.Rows[i].Cells["name"].Value = textBox_user_name.Text;
                dataGridView_users.Rows[i].Cells["group"].Value = inc.User_Group[comboBox_user_group.SelectedIndex].ToString();
                dataGridView_users.Rows[i].Cells["pwd"].Value = textBox_user_pwd.Text;
                dataGridView_users.Rows[i].Cells["times"].Value = textBox_user_times.Text;

                if (textBox_user_times.Text == "")
                    textBox_user_times.Text = "0";
                inc.db_control.SQL = inc.db_control.insert_table("tb_users", textBox_user_id.Text
                    , textBox_user_name.Text, Tools.UserMd5(textBox_user_pwd.Text)
                    , (comboBox_user_group.SelectedIndex + 1).ToString(), textBox_user_times.Text, "1800-01-01 00:00:00", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.USI01, textBox_user_name.Text, 1);
                    fn_global.load_user_table();
                    load_users_dataview();
                    MessageBox.Show("Add User successful");
                }
                else
                    MessageBox.Show("Add User Failed");
            }
            catch { MessageBox.Show("Add User Failed"); }
        }

        private void button29_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Delete the user?", "User Config", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    int i = dataGridView_users.CurrentRow.Index;

                    inc.db_control.SQL = inc.db_control.delete_table("tb_users");
                    inc.db_control.SQL += " where fd_id='" + dataGridView_users.Rows[i].Cells["id"].Value.ToString() + "'";
                    if (inc.db_control.SQLExecuteReader() != null)
                    {
                        fn_global.log_operateion((int)inc.LOGMSGCODE.USD01, textBox_user_name.Text, 1);
                        dataGridView_users.Rows.Remove(dataGridView_users.CurrentRow);
                        fn_global.load_user_table();                        
                        MessageBox.Show("Delete User successful");
                    }
                    else
                        MessageBox.Show("Delete User Failed");
                }
                catch { MessageBox.Show("Delete User Failed"); }
            }
        }
        private void button30_Click(object sender, EventArgs e)
        {
            if (textBox_user_pwd.Text.Length < 8)
            {
                MessageBox.Show("At least 8 Charaters");
                return;
            }

            if (MessageBox.Show("Save the User Information?", "User Config", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    bool _changed_pwd = false;
                    int i = dataGridView_users.CurrentRow.Index;
                    //dataGridView_users.Rows[i].Cells["Id"].Value = textBox_user_id.Text;
                    dataGridView_users.Rows[i].Cells["name"].Value = textBox_user_name.Text;
                    dataGridView_users.Rows[i].Cells["group"].Value = inc.User_Group[comboBox_user_group.SelectedIndex].ToString();
                    if (dataGridView_users.Rows[i].Cells["pwd"].Value != textBox_user_pwd.Text)
                    {
                        _changed_pwd = true;
                        dataGridView_users.Rows[i].Cells["pwd"].Value = textBox_user_pwd.Text;
                    }
                    dataGridView_users.Rows[i].Cells["times"].Value = textBox_user_times.Text;

                    if (dataGridView_users.Rows[i].Cells["pwd"].Value.ToString() != textBox_user_pwd.Text)
                    {
                        fn_global.udpate_users_password_time((int)dataGridView_users.Rows[i].Cells["Id"].Value);
                    }
                    string _t_pwd = textBox_user_pwd.Text;
                    if (_changed_pwd)
                        _t_pwd = Tools.UserMd5(textBox_user_pwd.Text);

                    inc.db_control.SQL = inc.db_control.udpate_table("tb_users", textBox_user_id.Text
                        , textBox_user_name.Text, _t_pwd
                        , (comboBox_user_group.SelectedIndex + 1).ToString(), "0");



                    inc.db_control.SQL += " where fd_id='" + dataGridView_users.Rows[i].Cells["Id"].Value.ToString() + "'";
                    if (inc.db_control.SQLExecuteReader() != null)
                    {
                        fn_global.log_operateion((int)inc.LOGMSGCODE.USU01, textBox_user_name.Text, 1);
                        MessageBox.Show("Update user Successful");
                        fn_global.load_user_table();
                        load_users_dataview();
                    }
                    else
                        MessageBox.Show("Update User Failed");
                }
                catch { MessageBox.Show("Update User Failed"); }
            }
        }
        private void dataGridView_users_SelectionChanged(object sender, EventArgs e)
        {
            int i = dataGridView_users.CurrentRow.Index;
            textBox_user_id.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["Id"].Value);
            textBox_user_name.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["name"].Value);
            int _index = 0;
            for (int j = 0; j < inc.User_Group.Length; j++)
            {
                string _s = Tools.get_value(dataGridView_users.Rows[i].Cells["group"].Value);
                if (_s == inc.User_Group[j].ToString())
                {
                    _index = j;
                    break;
                }
            }
            comboBox_user_group.SelectedIndex = _index;
            textBox_user_pwd.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["pwd"].Value);
            textBox_user_times.Text = Tools.get_value(dataGridView_users.Rows[i].Cells["times"].Value);

        }

        private void load_user_rights_group()
        {
            int menu_level = 1;
            if (comboBox1.Text == "Supervisor")
                menu_level = 2;
            int _c_level = inc.TB_user[fn_global.fn_user_id2index(inc.currentuser)].level;
            listBox2.Items.Clear();
            listBox1.Items.Clear();
            //左边的大于操作组的权限，小于当前用户的权限，右边的是当前用户组权限
            inc.db_control.SQL = "select * from tb_menu where fd_level<='" + menu_level + "' and fd_delete<>1 and fd_level>0  order by fd_new_id";
            OdbcDataReader MyReader1 = inc.db_control.SQLExecuteReader();
            if (MyReader1 != null)
            {
                while (MyReader1.Read())
                {
                    listBox2.Items.Add((string)MyReader1["fd_name"]);
                }
            }

            inc.db_control.SQL = "select * from tb_menu where fd_level>'" + menu_level + "' and fd_level<='"
                + _c_level + "' and fd_delete<>1 and fd_level>0 order by fd_new_id";
            MyReader1 = inc.db_control.SQLExecuteReader();
            if (MyReader1 != null)
            {
                while (MyReader1.Read())
                {
                    listBox1.Items.Add((string)MyReader1["fd_name"]);
                }
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            load_user_rights_group();
        }

        private void button51_Click(object sender, EventArgs e)
        {
            try
            {
                int menu_level = 1;
                if (comboBox1.Text == "Supervisor")
                    menu_level = 2;
                string _name = listBox1.Items[listBox1.SelectedIndex].ToString();
                inc.db_control.SQL = "update tb_menu set fd_level='" + menu_level + "' where fd_name='" + _name + "' and fd_delete<>1 and fd_level>0";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.MNU01, "add " + _name, 1);
                    load_user_rights_group();
                }
                else
                    MessageBox.Show("Modify User Rights Failed");

            }
            catch { MessageBox.Show("Modify User Rights Failed"); }

        }
        private void button50_Click(object sender, EventArgs e)
        {
            try
            {
                int menu_level = 1;
                if (comboBox1.Text == "Supervisor")
                    menu_level = 2;
                string _name = listBox2.Items[listBox2.SelectedIndex].ToString();

                inc.db_control.SQL = "update tb_menu set fd_level='" + (menu_level + 1) + "' where fd_name='" + _name + "' and fd_delete<>1 and fd_level>0";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.MNU01, "delete " + _name, 1);
                    load_user_rights_group();
                }
                else
                    MessageBox.Show("Modify User Rights Failed");
            }
            catch { MessageBox.Show("Modify User Rights Failed"); }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            textBox_user_times.Text = "0";
            int i = dataGridView_users.CurrentRow.Index;

            //inc.db_control.SQL = inc.db_control.udpate_table("tb_users", textBox_user_id.Text
            //   , textBox_user_name.Text,  Tools.UserMd5(textBox_user_pwd.Text)
            //   , (comboBox_user_group.SelectedIndex + 1).ToString(), textBox_user_times.Text);
            inc.db_control.SQL = "update tb_users set fd_trytimes=0";
            inc.db_control.SQL += " where fd_id='" + dataGridView_users.Rows[i].Cells["Id"].Value.ToString() + "'";
            try
            {
                inc.db_control.SQLExecuteReader();
                MessageBox.Show("Reset User Successful");
            }
            catch
            {
                MessageBox.Show("Reset User Failed");
            }
        }
        #endregion

        #region Global Paramiter

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void _key_press(object sender,KeyPressEventArgs e)
        //{
        //    if (e.KeyChar != ':' && e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
        //    {
        //        e.Handled = true;
        //    }
        //}
        private void init_global_par()
        {
            dataGridView_global.Columns.Add("Id", "ID");
            dataGridView_global.Columns.Add("name", "Name");
            dataGridView_global.Columns.Add("remark", "Description");
            dataGridView_global.Columns.Add("Value", "Value");

            dataGridView_global.Columns["name"].Width = 200;
            dataGridView_global.Columns["remark"].Width = 200;
            dataGridView_global.Columns["Value"].Width = 200;
        }
        private bool get_onoff_config()
        {
            if (fn_global.fn_get_file("setting", "cms_timezone.txt", "cms_timezone.txt"))
            {
                for (int i = 0; i < inc.tb_area.Length; i++)
                {
                    string name = "\\cms_onoff." + inc.tb_area[i].fd_id.ToString() + ".txt";
                    if (!(fn_global.fn_get_file("setting", name, name)))
                        MessageBox.Show("Get file " + name + " Failed");
                }
                return true;
            }
            return false;
        }
        private bool put_onoff_config()
        {
            int index = fn_global.fn_area_name2index(comboBox_areas.Text);
            int id = inc.tb_area[index].areaid;

            if (fn_global.fn_put_file("setting", "\\cms_onoff." + id.ToString() + ".txt"))
                return fn_global.fn_put_file("setting", "\\cms_timezone.txt");
            return false;
        }

        private void load_onoff_config()
        {
            if (get_onoff_config())
            {
                string path = fn_global.get_workdir("setting");

                int index = fn_global.fn_area_name2index(comboBox_areas.Text);
                int id = inc.tb_area[index].areaid;

                MyFile fonoff = new MyFile(path + "\\cms_onoff." + id.ToString() + ".txt");
                string _temp = "";
                while ((_temp = fonoff.read_file_line()) != null)
                {
                    int _index = 4;
                    try
                    {
                        string _day = _temp.Substring(7, 1);
                        int i = int.Parse(_day) * 2;
                        if (_temp.IndexOf("on") > 0)
                        {
                            _index = _index + i - 1;
                            //((TextBox)this.Controls.Find("textBox" + _index, true)[0]).Text = get_str_value(_temp);
                            try
                            {
                                ((DateTimePicker)this.Controls.Find("dateTimePicker" + _index, true)[0]).Text = get_str_value(_temp);
                            }
                            catch { }
                        }
                        else if (_temp.IndexOf("off") > 0)
                        {
                            _index = _index + i;
                            //((TextBox)this.Controls.Find("textBox" + _index, true)[0]).Text = get_str_value(_temp);
                            try
                            {
                                ((DateTimePicker)this.Controls.Find("dateTimePicker" + _index, true)[0]).Text = get_str_value(_temp);
                            }
                            catch { }
                        }
                    }
                    catch { }

                }
                fonoff.close();
            }
        }
        private void load_global_par()
        {
            inc.db_control.SQL = "select fd_id,fd_name,fd_remark,fd_value,fd_order from tb_setting order by fd_order";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            dataGridView_global.Rows.Clear();
            int i = 0;
            while (MyReader.Read())
            {
                dataGridView_global.Rows.Add();
                dataGridView_global.Rows[i].Cells["Id"].Value = (Int64)MyReader["fd_id"];
                dataGridView_global.Rows[i].Cells["name"].Value = (String)MyReader["fd_name"];
                dataGridView_global.Rows[i].Cells["remark"].Value = (String)MyReader["fd_remark"];
                dataGridView_global.Rows[i].Cells["Value"].Value = (String)MyReader["fd_value"];

                i++;
            }
            comboBox_areas.SelectedIndex = 0;
            load_onoff_config();

            string path = fn_global.get_workdir("setting");

            MyFile ftimezone = new MyFile(path + "\\cms_timezone.txt");
            string _temp = "";
            int j = 0;
            int _len = 1;
            while ((_temp = ftimezone.read_file_line()) != null)
            {
                int _index = 22;
                try
                {
                    bool _addj = false;
                    if (j >= 10)
                        _len = 2;
                    string _day = _temp.Substring(6, _len);
                    i = int.Parse(_day) * 2;
                    if (_temp.IndexOf("start") > 0)
                    {
                        _index = _index + i - 1;
                        //((TextBox)this.Controls.Find("textBox" + _index, true)[0]).Text = get_str_value(_temp);
                        try
                        {
                            ((DateTimePicker)this.Controls.Find("dateTimePicker" + _index, true)[0]).Text = get_str_value(_temp);
                        }
                        catch { }
                        _addj = true;
                    }
                    else if (_temp.IndexOf("end") > 0)
                    {
                        _index = _index + i;
                        //((TextBox)this.Controls.Find("textBox" + _index, true)[0]).Text = get_str_value(_temp);
                        try
                        {
                            ((DateTimePicker)this.Controls.Find("dateTimePicker" + _index, true)[0]).Text = get_str_value(_temp);
                        }
                        catch { }
                        _addj = true;
                    }
                    if (_addj)
                        j++;
                }
                catch { }
            }
            ftimezone.close();

        }

        private void comboBox_areas_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int index = fn_global.fn_area_name2index(((ComboBox)sender).Text);
            //int id = inc.tb_area[index].areaid;
            //string _workdir = fn_global.get_workdir("setting");

            load_onoff_config();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < dataGridView_global.Rows.Count; i++)
                {
                    string id = Tools.get_value(dataGridView_global.Rows[i].Cells["Id"].Value);
                    string remark = Tools.get_value(dataGridView_global.Rows[i].Cells["remark"].Value);
                    string value = Tools.get_value(dataGridView_global.Rows[i].Cells["Value"].Value);
                    inc.db_control.SQL = "update tb_setting set fd_remark='" + remark + "',fd_value='" + value + "' where fd_id='" + id + "'";
                    try { inc.db_control.SQLExecuteReader(); }
                    catch { }
                }

                string path = fn_global.get_workdir("setting");

                //if (File.Exists(path + "\\cms_timezone.txt"))
                //    File.Delete(path + "\\cms_timezone.txt");
                if (fn_global.fn_file_exist(path, "cms_timezone.txt"))
                    fn_global.fn_file_exist(path, "cms_timezone.txt");

                MyFile ftimezone = new MyFile(path + "\\cms_timezone.txt");
                int _index = 0;
                for (int i = 23; i < 55; i++)
                {
                    _index = 22;
                    _index = (i - _index - 1) / 2 + 1;
                    if (i % 2 == 1)
                    {
                        //ftimezone.write_to_file("cms_tz" + _index.ToString() + "_start=" + ((TextBox)this.Controls.Find("textBox" + i, true)[0]).Text;                    
                        ftimezone.write_to_file("cms_tz" + _index.ToString() + "_start=" + ((DateTimePicker)this.Controls.Find("dateTimePicker" + i, true)[0]).Text);
                    }
                    else
                    {
                        //ftimezone.write_to_file("cms_tz" + _index.ToString() + "_end=" + ((TextBox)this.Controls.Find("textBox" + i, true)[0]).Text);
                        ftimezone.write_to_file("cms_tz" + _index.ToString() + "_end=" + ((DateTimePicker)this.Controls.Find("dateTimePicker" + i, true)[0]).Text);
                    }
                }
                ftimezone.write_to_file("file_type=cms");
                ftimezone.close();

                int index = fn_global.fn_area_name2index(comboBox_areas.Text);
                int areaid = inc.tb_area[index].areaid;

                //if (File.Exists(path + "\\cms_onoff." + areaid.ToString() + ".txt"))
                //    File.Delete(path + "\\cms_onoff." + areaid.ToString() + ".txt");

                if (fn_global.fn_file_exist(path, "cms_onoff." + areaid.ToString() + ".txt"))
                    fn_global.fn_file_delete(path, "cms_onoff." + areaid.ToString() + ".txt");

                MyFile fonoff = new MyFile(path + "\\cms_onoff." + areaid.ToString() + ".txt");

                for (int i = 5; i < 23; i++)
                {
                    _index = 4;
                    _index = (i - _index - 1) / 2 + 1;
                    if (i % 2 == 1)
                        fonoff.write_to_file("cms_day" + _index.ToString() + "_on_time=" + ((DateTimePicker)this.Controls.Find("dateTimePicker" + i, true)[0]).Text);
                    else
                        fonoff.write_to_file("cms_day" + _index.ToString() + "_off_time=" + ((DateTimePicker)this.Controls.Find("dateTimePicker" + i, true)[0]).Text);
                }
                fonoff.write_to_file("file_type=cms");
                fonoff.close();

                if (put_onoff_config())
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.STU01, "", 1);

                    MessageBox.Show("Update Global parameters successful");
                }
                else
                    MessageBox.Show("Update Global parameters failed");
            }
            catch { MessageBox.Show("Update Global parameters failed"); }
        }

        #endregion

        #region schedule config
        private bool schedule_changed = false;
        private void Initialize_schedule()
        {
            comboBox_schdule.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(inc.WeekDays_Holiday);
            comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox36.Items.Clear();
            comboBox36.Items.AddRange(inc.VMS_Flash_mode);
            comboBox36.DropDownStyle = ComboBoxStyle.DropDownList;

            comboBox38.Items.Clear();
            comboBox38.Items.AddRange(inc.VMS_Name_color);
            comboBox38.DropDownStyle = ComboBoxStyle.DropDownList;

            treeView_msg.NodeMouseDoubleClick += new TreeNodeMouseClickEventHandler(double_click_msgtree);
            treeView_msg.LostFocus += new EventHandler(mouse_leave);
            groupBox1.Controls.Add(treeView_msg);


            button25.Enabled = false;
            button26.Enabled = false;
            button9.Enabled = false;

            for (int j = 101; j < 119; j++)
            {
                ((ComboBox)this.Controls.Find("comboBox" + j, true)[0]).DropDown += new EventHandler(click_msg_combobox);
                ((ComboBox)this.Controls.Find("comboBox" + j, true)[0]).DropDownHeight = 1;
            }
            fn_global.load_msg_treeview(treeView_msg);
            for (int j = 5; j <= 22; j++)
            {
                ((ComboBox)this.Controls.Find("comboBox" + j, true)[0]).Items.AddRange(inc.VMS_Name_color);
                ((ComboBox)this.Controls.Find("comboBox" + j, true)[0]).DropDownStyle = ComboBoxStyle.DropDownList;
                ((ComboBox)this.Controls.Find("comboBox" + j, true)[0]).SelectedIndexChanged += new EventHandler(click_color_combobox);
            }
            for (int j = 1; j <= 6; j++)
            {
                ((CheckBox)this.Controls.Find("checkBox" + j, true)[0]).CheckedChanged += new EventHandler(check_changed_checkbox);
            }
        }

        private void update_site()
        {
            string _name = "Update Adhoc Schedule ";
            string _title = "Update Adhoc Schedule ";
            string _path = "";
            string _file = "update.txt";
            string _cmd = "";

            string _str = "";
            this.Cursor = Cursors.WaitCursor;

            if (System.Configuration.ConfigurationSettings.AppSettings["tcpdemo"] != "1")
                fn_global.fn_file_delete(_path, _file);

            _cmd = "<update_adhoc>";
            for (int m = 0; m < 5; m++)
            {
                Thread.Sleep(1000);
                Application.DoEvents();
            }
            if (fn_global.fn_tcp_send(_cmd))
            {
                if (fn_global.fn_cmd_check_finish(_path, _file, ref _str))
                {
                    fn_global.fn_return_status(_str, _name, _title, 1);
                }

            }
            this.Cursor = Cursors.Default;
        }
        private void refresh_schedule()
        {
            int i = comboBox_schdule.SelectedIndex;
            init_schedule();
            try
            {
                comboBox_schdule.SelectedIndex = i;
            }
            catch
            {
                comboBox_schdule.SelectedIndex = -1;
            }
        }

        private void clear_adhoc_screen()
        {
            try
            {
                radioButton1.Checked = true;
                label14.Enabled = false;
                comboBox2.Enabled = false;

                treeView_msg.Visible = false;

                comboBox36.SelectedIndex = 0;
                textBox4.Text = "2";
                listBox_schedule.Items.Clear();
                listBox3.Items.Clear();
                listBox_timezone.Items.Clear();
                listBox_timezone_group.Items.Clear();

                comboBox_schdule.Items.Clear();

                for (int i = 1; i < 19; i++)
                {
                    ((ComboBox)this.Controls.Find("comboBox" + (i + 100), true)[0]).Text = "";
                    ((CheckBox)this.Controls.Find("checkBox" + i, true)[0]).Checked = false;
                }
                comboBox38.SelectedIndex = 0;
            }
            catch { }
        }
        private void init_schedule()
        {
            clear_adhoc_screen();

            inc.db_control.SQL = "select * from tb_schedule where fd_delete<>1 order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            if (MyReader != null)
            {
                inc.tb_schedule = new inc.schedule[MyReader.RecordsAffected];
                int i = 0;

                while (MyReader.Read())
                {
                    inc.tb_schedule[i].id = (Int32)MyReader["fd_id"];
                    inc.tb_schedule[i].name = (String)MyReader["fd_name"];
                    inc.tb_schedule[i].starttime = (DateTime)MyReader["fd_starttime"];
                    inc.tb_schedule[i].endtime = (DateTime)MyReader["fd_endtime"];
                    inc.tb_schedule[i].useendtime = (Int32)MyReader["fd_useendtime"];

                    string gdms = ((String)MyReader["fd_gdm"]).Trim();

                    inc.tb_schedule[i].gdm_list = new List<string>();

                    string[] gdms_array = gdms.Split(',');
                    for (int m = 0; m < gdms_array.Length; m++)
                    {
                        if (gdms_array[m].Trim() != "")
                            inc.tb_schedule[i].gdm_list.Add(gdms_array[m].Trim());
                    }
                    try
                    {
                        inc.tb_schedule[i].datezone = (string)MyReader["fd_template"];
                    }
                    catch { }

                    inc.tb_schedule[i].timezone_group = new List<inc.timezone_group_msg_str>();

                    inc.db_control.SQL = "select * from tb_schedule_msg where fd_schedule_id='" + inc.tb_schedule[i].id.ToString()
                        + "' and fd_delete=0 order by fd_id";
                    OdbcDataReader MyReader2 = inc.db_control.SQLExecuteReader();
                    if (MyReader2 != null)
                    {
                        while (MyReader2.Read())
                        {
                            inc.timezone_group_msg_str _tzgmsg = new inc.timezone_group_msg_str();
                            _tzgmsg.id = (Int64)MyReader2["fd_id"];
                            _tzgmsg.schedule_id = (Int64)MyReader2["fd_schedule_id"];

                            _tzgmsg.msg = MyReader2["fd_msg"] != DBNull.Value ? (String)MyReader2["fd_msg"] : "";
                            _tzgmsg.timezones = MyReader2["fd_timezone"] != DBNull.Value ? (String)MyReader2["fd_timezone"] : "";

                            _tzgmsg.mode = (Int64)MyReader2["fd_mode"];
                            _tzgmsg.timer = (Int64)MyReader2["fd_timer"];

                            _tzgmsg.pattern = 0;
                            _tzgmsg.pattern = MyReader2["fd_pattern"] != DBNull.Value ? (Byte)MyReader2["fd_pattern"] : _tzgmsg.pattern;
                            inc.tb_schedule[i].timezone_group.Add(_tzgmsg);
                        }
                    }


                    comboBox_schdule.Items.Add(inc.tb_schedule[i].name);
                    i++;
                }
            }


        }
        /// <summary>
        /// user changed color of message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void click_color_combobox(object sender, EventArgs e)
        {
            int _index = int.Parse(((ComboBox)sender).Name.Substring(8)) + 96;

            if (((ComboBox)sender).SelectedIndex == 0)
                ((ComboBox)this.Controls.Find("comboBox" + _index, true)[0]).ForeColor = Color.Green;
            else if (((ComboBox)sender).SelectedIndex == 1)
                ((ComboBox)this.Controls.Find("comboBox" + _index, true)[0]).ForeColor = inc.Amber;
            else
                ((ComboBox)this.Controls.Find("comboBox" + _index, true)[0]).ForeColor = Color.Red;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void check_changed_checkbox(object sender, EventArgs e)
        {
            if (check_pages1_change)
            {
                //check_pages1_change = false;
            }
            else
            {
                int _index = int.Parse(((CheckBox)sender).Name.Substring(8)) + 6;
                ((CheckBox)this.Controls.Find("checkBox" + _index, true)[0]).Checked = ((CheckBox)sender).Checked;
                _index += 6;
                ((CheckBox)this.Controls.Find("checkBox" + _index, true)[0]).Checked = ((CheckBox)sender).Checked;
            }
        }

        private void double_click_msgtree(object sender, TreeNodeMouseClickEventArgs e)
        {
            string parent_text = "", node_text = "";
            fn_global.Auto_close_treeview((TreeView)sender, ref parent_text, ref node_text);
            _set_adhoc_msg(node_text, current_combobox);
        }

        private void click_msg_combobox(object sender, EventArgs e)
        {
            fn_global.show_treeview_location(treeView_msg, ((ComboBox)sender).Left, ((ComboBox)sender).Top + ((ComboBox)sender).Height);
            current_combobox = ((ComboBox)sender).Name;
        }

        /// <summary>
        /// set adhoc message get grom premessage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _set_adhoc_msg(string pre_name, string combobox_name)
        {

            int _index = fn_global.fn_msg_name2index(pre_name);

            if (_index >= 0)
            {
                try
                {
                    int _line = (int.Parse(combobox_name.Substring(8)) - 1 - 100) / 6;
                    int _page = (int.Parse(combobox_name.Substring(8)) - 1 - 100) % 6;

                    string[] msgs = new string[6];
                    msgs[0] = inc.Pre_Msg[_index].line1;
                    msgs[1] = inc.Pre_Msg[_index].line2;
                    msgs[2] = inc.Pre_Msg[_index].line3;
                    msgs[3] = inc.Pre_Msg[_index].line4;
                    msgs[4] = inc.Pre_Msg[_index].line5;
                    msgs[5] = inc.Pre_Msg[_index].line6;

                    int len = 0;
                    for (int i = 5; i >= 0; i--)
                    {
                        if (msgs[i] != "")
                        {
                            len = i;
                            break;
                        }
                    }

                    for (int i = _page; i < 6; i++)
                    {
                        if ((i - _page) > len)
                            break;
                        ((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + i + 1 + 100), true)[0]).Text = msgs[i - _page];
                    }

                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 1 + 100), true)[0]).Text = inc.Pre_Msg[_index].line1;
                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 2 + 100), true)[0]).Text = inc.Pre_Msg[_index].line2;
                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 3 + 100), true)[0]).Text = inc.Pre_Msg[_index].line3;
                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 4 + 100), true)[0]).Text = inc.Pre_Msg[_index].line4;
                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 5 + 100), true)[0]).Text = inc.Pre_Msg[_index].line5;
                    //((ComboBox)this.Controls.Find("comboBox" + (_line * 6 + 6 + 100), true)[0]).Text = inc.Pre_Msg[_index].line6;
                }
                catch { }
            }
        }

        private void select_schedule(int index)
        {
            listBox_schedule.Items.Clear();
            string vms_group = "";

            for (int j = 0; j < inc.tb_schedule[index].gdm_list.Count; j++)
            {
                int _gdmid = int.Parse(inc.tb_schedule[index].gdm_list[j]);
                int _index = fn_global.fn_vms_id2index(_gdmid);

                if (_index >= 0)
                {
                    listBox_schedule.Items.Add(inc._vms[_index].name);
                }
            }

            for (int i = 0; i < inc._vms.Length; i++)
            {
                for (int m = 0; m < inc.tb_schedule.Length; m++)
                {
                    for (int n = 0; n < inc.tb_schedule[m].gdm_list.Count; n++)
                    {
                        if (inc._vms[i]._id.ToString() == inc.tb_schedule[m].gdm_list[n])
                            vms_group += inc._vms[i].name + ",";
                    }
                }
            }

            int _w = treeView1.Width;
            int _h = treeView1.Height;
            fn_global.update_vms_treeview(treeView1, vms_group);
            treeView1.Height = _h;
            treeView1.Width = _w;

            listBox3.Items.Clear();

            string[] date_group = inc.tb_schedule[index].datezone.Split('%');

            for (int j = 0; j < date_group.Length; j++)
            {
                if (date_group[j].Trim() != "")
                    listBox3.Items.Add(date_group[j]);
            }

            listBox_timezone_group.Items.Clear();
            for (int j = 0; j < inc.tb_schedule[index].timezone_group.Count; j++)
            {
                string _temp = inc.tb_schedule[index].timezone_group[j].id.ToString();
                _temp += "|" + inc.tb_schedule[index].timezone_group[j].timezones;
                listBox_timezone_group.Items.Add("ID:" + _temp);
            }
            schedule_changed = true;
            listBox_timezone_group.SelectedIndex = inc.tb_schedule[index].timezone_group.Count > 0 ? 0 : -1;

        }

        private void comboBox_schdule_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex >= 0)
            {
                button9.Enabled = true;
                select_schedule(((ComboBox)sender).SelectedIndex);
            }
            else
            {
                button9.Enabled = false;
            }

            schedule_changed = true;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Nodes.Count == 0)
                {
                    listBox_schedule.Items.Add(treeView1.SelectedNode.Text);
                    treeView1.SelectedNode.Remove();
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            int i = listBox_schedule.SelectedIndex;

            if (i >= 0)
            {
                string vms_name = listBox_schedule.Items[i].ToString();
                int index = fn_global.fn_vms_name2index(vms_name);
                if (index >= 0)
                {
                    int _area_index = fn_global.fn_area_id2index(inc._vms[index].area);
                    string nodes_name = inc.tb_area[_area_index].name;
                    for (int j = 0; j < treeView1.Nodes[0].Nodes.Count; j++)
                    {
                        if (treeView1.Nodes[0].Nodes[j].Text == nodes_name)
                        {
                            treeView1.Nodes[0].Nodes[j].Nodes.Add(vms_name);
                            break;
                        }
                    }

                    listBox_schedule.Items.Remove(listBox_schedule.Items[i]);
                }
            }
        }

        private void udpate_timezone_group()
        {
            int j = listBox_timezone_group.SelectedIndex;
            if (j >= 0)
            {
                string _temp = listBox_timezone_group.Items[j].ToString();
                string _time_zon = _temp.Substring(0, _temp.IndexOf("|") + 1);
                for (int i = 0; i < listBox_timezone.Items.Count; i++)
                {
                    if (i > 0)
                        _time_zon += ",";
                    _time_zon += listBox_timezone.Items[i].ToString();
                }

                listBox_timezone_group.Items[j] = _time_zon;
            }
        }

        private bool add_time_zone()
        {
            if (dateTimePicker1.Value >= dateTimePicker2.Value)
            {
                MessageBox.Show("Error Time");
                return false;
            }

            if (listBox_timezone.Items.Count >= 16)
            {
                MessageBox.Show("At most 16 TimeZones");
                return false;
            }
            else
            {
                for (int i = 0; i < listBox_timezone.Items.Count; i++)
                {
                    string[] _time = (listBox_timezone.Items[i].ToString()).Split('-');

                    if (_time.Length > 1)
                    {
                        DateTime t1 = DateTime.ParseExact(_time[0], "HH:mm:ss", null);
                        DateTime t2 = DateTime.ParseExact(_time[1], "HH:mm:ss", null);
                        if ((dateTimePicker1.Value >= t1) && (dateTimePicker1.Value < t2) || (dateTimePicker2.Value > t1) && (dateTimePicker2.Value <= t2))
                        {
                            MessageBox.Show("Error Time");
                            return false;
                        }
                    }

                }

                for (int i = 0; i < listBox_timezone_group.Items.Count; i++)
                {
                    string _temp = listBox_timezone_group.Items[i].ToString();
                    _temp = _temp.Substring(_temp.IndexOf("|") + 1);
                    if (_temp.Trim() == "")
                        continue;
                    string[] _time_zone = _temp.Split(',');

                    for (int j = 0; j < _time_zone.Length; j++)
                    {
                        string[] _time = (_time_zone[j].ToString()).Split('-');
                        DateTime t1 = DateTime.ParseExact(_time[0], "HH:mm:ss", null);
                        DateTime t2 = DateTime.ParseExact(_time[1], "HH:mm:ss", null);
                        if ((dateTimePicker1.Value >= t1) && (dateTimePicker1.Value < t2) || (dateTimePicker2.Value > t1) && (dateTimePicker2.Value <= t2))
                        {
                            MessageBox.Show("Error Time");
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        /// <summary>
        /// add time zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button25_Click(object sender, EventArgs e)
        {
            if (add_time_zone())
            {
                if (listBox_timezone.Items.Count == 1)
                {
                    if (listBox_timezone.Items[0].ToString().Trim() == "")
                    {
                        listBox_timezone.Items.Clear();
                    }
                }
                listBox_timezone.Items.Add(dateTimePicker1.Value.ToString("HH:mm:ss") + "-" + dateTimePicker2.Value.ToString("HH:mm:ss"));
                udpate_timezone_group();
            }
        }

        /// <summary>
        /// delete time zone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button26_Click(object sender, EventArgs e)
        {
            int i = listBox_timezone.SelectedIndex;
            if (i >= 0)
            {
                listBox_timezone.Items.Remove(listBox_timezone.Items[i]);
                udpate_timezone_group();
            }
        }

        /// <summary>
        /// add group 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button27_Click(object sender, EventArgs e)
        {
            if (!add_time_zone())
            {
                return;
            }
            if (listBox_timezone_group.Items.Count >= 16)
            {
                MessageBox.Show("At most 16 TimeZone Groups");
                listBox_timezone_group.SelectedIndex = 0;
            }
            else
            {
                string _time_zon = dateTimePicker1.Value.ToString("HH:mm:ss") + "-" + dateTimePicker2.Value.ToString("HH:mm:ss");
                listBox_timezone.Items.Clear();
                listBox_timezone.Items.Add(_time_zon);
                bool _already_in = false;
                for (int i = 0; i < listBox_timezone_group.Items.Count; i++)
                {
                    if (_time_zon == listBox_timezone_group.Items[i].ToString())
                    {
                        _already_in = true;
                    }
                }
                if (!_already_in)
                {
                    listBox_timezone_group.Items.Add("ID:-1|" + _time_zon);
                }

                //must select the new line,if not the group list will not update
                listBox_timezone_group.SelectedIndex = listBox_timezone_group.Items.Count - 1;
            }
        }

        /// <summary>
        /// delete group
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button28_Click(object sender, EventArgs e)
        {
            int i = listBox_timezone_group.SelectedIndex;
            if (i >= 0)
            {
                if (MessageBox.Show("Delete the Group?", "Delete Group", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    string _temp = listBox_timezone_group.Items[i].ToString();
                    string groupid = _temp.Substring(0, _temp.IndexOf("|"));
                    groupid = groupid.Substring(groupid.IndexOf(":") + 1);

                    inc.db_control.SQL = "update tb_schedule_msg set fd_delete=1 ";
                    inc.db_control.SQL += "where fd_id='" + groupid + "'";
                    inc.db_control.SQLExecuteReader();

                    listBox_timezone_group.Items.Remove(listBox_timezone_group.Items[i]);
                    listBox_timezone.Items.Clear();

                }
            }
        }

        private void show_timezone_group_message(string rows_data, int color)
        {
            int _color = color;
            List<String> rows_list = fn_global.fn_str2strlist(rows_data);
            for (int j = 1; j < 7; j++)
            {
                string s_key = "row" + j.ToString() + "_use";

                int i_use = 0;
                try { i_use = int.Parse(fn_global.fn_txt_getvalue(ref rows_list, s_key)); }
                catch { }

                for (int p = 0; p < 3; p++)
                {
                    int asss = (int)System.Math.Pow(2, p + 1);

                    int _num = j + p * 6;
                    if (Tools.fn_check_bit(asss, i_use))
                        ((CheckBox)this.Controls.Find("checkBox" + _num, true)[0]).Checked = true;
                    else
                        ((CheckBox)this.Controls.Find("checkBox" + _num, true)[0]).Checked = false;

                    s_key = "row" + j.ToString() + "_line" + (p + 1).ToString();
                    string str = fn_global.fn_txt_getvalue(ref rows_list, s_key);

                    _num += 4;

                    string _name_box = "comboBox" + _num.ToString();
                    ((ComboBox)this.Controls.Find(_name_box, true)[0]).Text = inc.VMS_Name_color[_color].ToString();

                    for (int c = 0; c < 3; c++)
                    {
                        if (str.IndexOf(inc.VMS_color_code[c].ToString()) >= 0)
                        {
                            ((ComboBox)this.Controls.Find(_name_box, true)[0]).Text = inc.VMS_Name_color[c].ToString();
                            str = str.Remove(str.IndexOf(inc.VMS_color_code[c].ToString()), 3);
                            break;
                        }
                    }

                    _num -= 4;
                    ((ComboBox)this.Controls.Find("comboBox" + (_num + 100), true)[0]).Text = str;
                }
            }
        }

        private string get_groupid(ref string group_timezones)
        {
            string groupid = group_timezones.Substring(0, group_timezones.IndexOf("|"));
            groupid = groupid.Substring(groupid.IndexOf(":") + 1);
            group_timezones = group_timezones.Substring(group_timezones.IndexOf("|") + 1);

            return groupid;
        }

        private void select_timezones(int index)
        {
            listBox_timezone.Items.Clear();
            string _temp = listBox_timezone_group.Items[index].ToString();
            string groupid = get_groupid(ref _temp);

            string[] _time_zone = _temp.Split(',');

            for (int j = 0; j < _time_zone.Length; j++)
            {
                listBox_timezone.Items.Add(_time_zone[j].ToString());
            }

            int _schedule_index = comboBox_schdule.SelectedIndex;
            if (_schedule_index >= 0)
            {
                for (int m = 0; m < inc.tb_schedule[_schedule_index].timezone_group.Count; m++)
                {
                    if (inc.tb_schedule[_schedule_index].timezone_group[m].id.ToString() == groupid)
                    {
                        show_timezone_group_message(inc.tb_schedule[_schedule_index].timezone_group[m].msg, 0);
                        textBox4.Text = inc.tb_schedule[_schedule_index].timezone_group[m].timer.ToString();
                        comboBox36.SelectedIndex = (int)inc.tb_schedule[_schedule_index].timezone_group[m].mode - 1;
                        checkBox_pattern.Checked = inc.tb_schedule[_schedule_index].timezone_group[m].pattern == 1 ? true : false;
                    }
                }

            }

        }

        private void save_schedule_group(int index)
        {
            try
            {
                int _schedule_index = comboBox_schdule.SelectedIndex;
                if (_schedule_index >= 0)
                {
                    string _temp = listBox_timezone_group.Items[index].ToString();
                    string groupid = get_groupid(ref _temp);

                    inc.timezone_group_msg_str _group = new inc.timezone_group_msg_str();

                    _group.msg = pack_rows();
                    _group.mode = comboBox36.SelectedIndex + 1;
                    _group.timer = long.Parse(textBox4.Text);
                    _group.schedule_id = inc.tb_schedule[_schedule_index].id;
                    _group.timezones = _temp;
                    if (checkBox_pattern.Checked)
                        _group.pattern = 1;
                    else
                        _group.pattern = 0;

                    if (groupid == "-1")
                    {
                        _group.id = -1;
                        inc.tb_schedule[_schedule_index].timezone_group.Add(_group);
                    }
                    else
                    {
                        _group.id = inc.tb_schedule[_schedule_index].timezone_group[index].id;
                        inc.tb_schedule[_schedule_index].timezone_group[index] = _group;
                    }
                }
            }
            catch { }
        }
        private void listBox_timezone_group_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (schedule_changed)
            {
                schedule_changed = false;
            }
            else
            {
                if (Cur_Schedule_Group >= 0)
                {
                    save_schedule_group(Cur_Schedule_Group);
                }
            }

            Cur_Schedule_Group = ((ListBox)sender).SelectedIndex;

            if (((ListBox)sender).SelectedIndex >= 0)
            {
                button25.Enabled = true;
                button26.Enabled = true;
                select_timezones(((ListBox)sender).SelectedIndex);
            }
            else
            {
                button25.Enabled = false;
                button26.Enabled = false;
            }
        }

        private void listBox_timezone_SelectedIndexChanged(object sender, EventArgs e)
        {
            int i = listBox_timezone.SelectedIndex;
            if (i >= 0)
            {
                string _temp = listBox_timezone.Items[i].ToString();
                if (_temp.Trim() != "")
                {
                    string[] _time = _temp.Split('-');
                    dateTimePicker1.Value = DateTime.ParseExact(_time[0], "HH:mm:ss", null);
                    dateTimePicker2.Value = DateTime.ParseExact(_time[1], "HH:mm:ss", null);
                }
            }
        }

        private void new_schedule()
        {
            init_schedule();
            comboBox_schdule.SelectedIndex = comboBox_schdule.Items.Count - 1;

        }
        private void button43_Click(object sender, EventArgs e)
        {
            Form_new_schedule f = new Form_new_schedule();
            f.MyEnvent += new CallBack_New_Schedule(new_schedule);
            f.ShowDialog();
        }

        private string pack_rows()
        {
            string s_3 = "";
            for (int i = 0; i < 6; i++)
            {
                string s_row = "row" + (i + 1).ToString() + "_";//row1_
                string _name = s_row + "use";               //row1_use
                string linemsg = "";
                int lineused = 0;
                for (int j = 0; j < 3; j++)
                {
                    int index = i + 1 + j * 6;
                    if (((CheckBox)this.Controls.Find("checkBox" + index, true)[0]).Checked)
                    {
                        lineused += (int)System.Math.Pow(2, j + 1);
                    }
                    int index_color = ((ComboBox)this.Controls.Find("comboBox" + (index + 4), true)[0]).SelectedIndex;
                    string _color = "";
                    if (index_color >= 0)
                        _color = inc.VMS_color_code[index_color].ToString();

                    string _msg = ((ComboBox)this.Controls.Find("comboBox" + (index + 100), true)[0]).Text;

                    //linemsg += s_row + "line" + (j + 1).ToString() + "=" + _color + _msg + "&";

                    _msg = fn_global.fn_rawurlencode(_msg);

                    if (_msg != "")
                        linemsg += s_row + "line" + (j + 1).ToString() + "=" + _color + _msg + inc.split_sign;
                    else
                        linemsg += s_row + "line" + (j + 1).ToString() + "=" + inc.split_sign;

                }
                s_3 += _name + "=" + lineused.ToString() + inc.split_sign + linemsg;
            }
            return s_3;
        }

        private void update_schedule(int index)
        {
            string fd_rows = pack_rows();
            string fd_mode = (comboBox36.SelectedIndex + 1).ToString();
            string fd_timer = textBox4.Text;
            string fd_name = comboBox_schdule.Text;
            string fd_delete = "0"; ;
            string fd_user = inc.currentuser.ToString();
            string fd_gdm = "";
            string fd_template = "";

            for (int i = 0; i < listBox3.Items.Count; i++)
            {
                fd_template += listBox3.Items[i].ToString() + "%";
            }

            for (int i = 0; i < listBox_schedule.Items.Count; i++)
            {
                string _temp = listBox_schedule.Items[i].ToString().Trim();
                if (_temp != "")
                {
                    int _index = fn_global.fn_vms_name2index(_temp);
                    fd_gdm += inc._vms[_index]._id + ",";
                }
            }


            inc.db_control.SQL = inc.db_control.udpate_table("tb_schedule", inc.tb_schedule[index].id.ToString(), fd_name, "0"
                , fd_delete, fd_user, fd_template, fd_gdm
                , inc.tb_schedule[index].starttime.ToString("yyyy-MM-dd HH:mm:ss")
                , inc.tb_schedule[index].endtime.ToString("yyyy-MM-dd HH:mm:ss")
                , inc.tb_schedule[index].useendtime.ToString());
            inc.db_control.SQL += "where fd_delete<>1 and fd_id='" + inc.tb_schedule[index].id.ToString() + "'";

            if (inc.db_control.SQLExecuteReader() != null)//udpate schedule time
            {

                for (int j = 0; j < listBox_timezone_group.Items.Count; j++)
                {
                    string _zone = listBox_timezone_group.Items[j].ToString();

                    string group_id = (_zone.Remove(_zone.IndexOf("|"))).Substring(3).Trim();
                    string _timezone = _zone.Substring(_zone.IndexOf("|") + 1).Trim();

                    if ((group_id != "") && (_timezone != ""))
                    {
                        if (group_id == "-1")
                        {
                            if (listBox_timezone_group.SelectedIndex == j)
                            {
                                inc.db_control.SQL = inc.db_control.insert_table("tb_schedule_msg", ""
                                    , inc.tb_schedule[index].id.ToString()
                                    , fd_rows, _timezone, "0", fd_mode
                                    , fd_timer, inc.tb_schedule[index].id.ToString()
                                    , (checkBox_pattern.Checked == true ? "1" : "0"));
                            }
                            else
                            {
                                try
                                {
                                    inc.db_control.SQL = inc.db_control.insert_table("tb_schedule_msg", ""
                                                                       , inc.tb_schedule[index].id.ToString()
                                                                       , inc.tb_schedule[index].timezone_group[j].msg
                                                                       , inc.tb_schedule[index].timezone_group[j].timezones
                                                                       , "0"
                                                                       , inc.tb_schedule[index].timezone_group[j].mode.ToString()
                                                                       , inc.tb_schedule[index].timezone_group[j].timer.ToString()
                                                                       , inc.tb_schedule[index].id.ToString()
                                                                       , inc.tb_schedule[index].timezone_group[j].pattern.ToString());
                                }
                                catch { MessageBox.Show("update schedule Failed"); }
                            }
                            if (inc.db_control.SQLExecuteReader() == null)
                            {
                                MessageBox.Show("update schedule Failed");
                            }
                            else
                                fn_global.log_operateion((int)inc.LOGMSGCODE.SCU01, inc.tb_schedule[index].name, 1);
                        }
                        else
                        {
                            if (listBox_timezone_group.SelectedIndex == j)
                            {
                                inc.db_control.SQL = "update tb_schedule_msg set fd_msg='" + fd_rows + "', fd_timezone='" + _timezone
                                    + "',fd_mode = '" + fd_mode
                                    + "',fd_timer='" + fd_timer
                                    + "',fd_pattern='" + (checkBox_pattern.Checked == true ? "1" : "0")
                                    + "' where fd_id='" + group_id + "'";
                            }
                            else
                            {
                                inc.db_control.SQL = "update tb_schedule_msg set fd_msg='" + inc.tb_schedule[index].timezone_group[j].msg
                                    + "', fd_timezone='" + inc.tb_schedule[index].timezone_group[j].timezones
                                    + "',fd_mode = '" + inc.tb_schedule[index].timezone_group[j].mode.ToString()
                                    + "',fd_timer='" + inc.tb_schedule[index].timezone_group[j].timer.ToString()
                                    + "',fd_pattern='" + inc.tb_schedule[index].timezone_group[j].pattern.ToString()
                                    + "' where fd_id='" + group_id + "'";
                            }
                            if (inc.db_control.SQLExecuteReader() == null)
                            {
                                MessageBox.Show("update schedule Failed");
                            }
                            else
                                fn_global.log_operateion((int)inc.LOGMSGCODE.SCU01, inc.tb_schedule[index].name, 1);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("update schedule Failed");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flag">
        /// 1 - for udpate/insert
        /// 2 - for delete
        /// </param>
        /// <returns></returns>
        private bool check_schedule_update(int flag)
        {
            int _schedule_index = comboBox_schdule.SelectedIndex;

            string _msg = "";
            if (flag == 1)
            {
                _msg = "Save Scedule";
                if (listBox_timezone_group.Items.Count <= 0)
                {
                    MessageBox.Show("At least one group");
                    return false;
                }
                if (comboBox36.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select Flash Mode");
                    return false;
                }
            }
            else
            {
                _msg = "Delete Scedule";
            }
            if (_schedule_index >= 0)
            {
                if (MessageBox.Show(_msg + "?", _msg, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    return true;
                }

            }
            else
            {
                MessageBox.Show("Please select a schedule");
            }
            return false;
        }
        private void button32_Click(object sender, EventArgs e)
        {
            if (check_schedule_update(1))
            {
                update_schedule(comboBox_schdule.SelectedIndex);

                //update_site();

                refresh_schedule();

            }
        }

        private void delete_schedule(int index)
        {
            try
            {
                inc.db_control.SQL = "update tb_schedule_msg set fd_delete=1 where fd_schedule_id='" + inc.tb_schedule[index].id.ToString() + "'";
                inc.db_control.SQLExecuteReader();

                inc.db_control.SQL = "update tb_schedule set fd_delete=1 where fd_id='" + inc.tb_schedule[index].id.ToString() + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.SCD01, inc.tb_schedule[index].name, 1);

                    MessageBox.Show("Delete Schedule Successful");
                }
                else
                    MessageBox.Show("Delete Schedule Failed");
            }
            catch { MessageBox.Show("Delete Schedule Failed"); }
        }

        private void button37_Click(object sender, EventArgs e)
        {

            if (check_schedule_update(2))
            {
                delete_schedule(comboBox_schdule.SelectedIndex);
                refresh_schedule();
            }
        }

        private bool check_date()
        {
            if (radioButton1.Checked)
            {
                if (dateTimePicker4.Value.DayOfYear > dateTimePicker3.Value.DayOfYear)
                {
                    MessageBox.Show("Error Date");
                    return false;
                }
                else
                {
                    for (int i = 0; i < listBox3.Items.Count; i++)
                    {
                        if (listBox3.Items[i].ToString().IndexOf("-") > 0)
                        {
                            string[] _time = listBox3.Items[i].ToString().Split('-');
                            DateTime t1 = Tools.str_to_datetime(_time[0], "dd/MM/yyyy");
                            DateTime t2 = Tools.str_to_datetime(_time[1], "dd/MM/yyyy");
                            if (((dateTimePicker4.Value < t1) && (dateTimePicker3.Value < t1)) || ((dateTimePicker4.Value > t2) && (dateTimePicker3.Value > t2)))
                            {

                            }
                            else
                                return false;
                        }
                    }
                }
            }
            else
            {
                if (comboBox2.SelectedIndex < 0)
                {
                    MessageBox.Show("Please select Date");
                    return false;
                }
                else
                {
                    for (int i = 0; i < listBox3.Items.Count; i++)
                    {
                        if (listBox3.Items[i].ToString() == comboBox2.Text)
                            return false;
                    }
                }
            }
            return true;
        }
        private void button39_Click(object sender, EventArgs e)
        {
            if (!check_date())
                return;
            if (radioButton1.Checked)
            {
                listBox3.Items.Add(dateTimePicker4.Text + "-" + dateTimePicker3.Text);
            }
            else
            {
                listBox3.Items.Add(comboBox2.Text);
            }
        }

        private void button38_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex >= 0)
                listBox3.Items.Remove(listBox3.Items[listBox3.SelectedIndex]);
        }


        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                label11.Enabled = true;
                label2.Enabled = true;
                dateTimePicker3.Enabled = true;
                dateTimePicker4.Enabled = true;
            }
            else
            {
                label11.Enabled = false;
                label2.Enabled = false;
                dateTimePicker3.Enabled = false;
                dateTimePicker4.Enabled = false;
            }
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                label14.Enabled = true;
                comboBox2.Enabled = true;
            }
            else
            {
                label14.Enabled = false;
                comboBox2.Enabled = false;
            }
        }

        private void listBox_schedule_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox_schedule.SelectedIndex >= 0)
            {
                fn_global.load_msg_treeview(treeView_msg, listBox_schedule.Items[listBox_schedule.SelectedIndex].ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            Form_Time_Config f = new Form_Time_Config();
            f._index_adhox = comboBox_schdule.SelectedIndex;
            f.isadhoc = false;
            f.Start_time = inc.tb_schedule[f._index_adhox].starttime;
            f.End_time = inc.tb_schedule[f._index_adhox].endtime;
            if (inc.tb_schedule[f._index_adhox].useendtime == 1)
                f.useendtime = true;
            else
                f.useendtime = false;
            f.Text = "Schedule Config Advance";

            f.ShowDialog();
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {

        }

        #endregion


        private void checkBox19_CheckedChanged(object sender, EventArgs e)
        {
            int index = int.Parse(((CheckBox)sender).Name.Substring(8)) - 18;
            if (index == 1)
                check_pages1_change = true;
            else
                check_pages1_change = false;

            for (int i = 0; i < 6; i++)
            {
                int j = (index - 1) * 6 + i + 1;
                ((CheckBox)this.Controls.Find("checkBox" + j, true)[0]).Checked = ((CheckBox)sender).Checked;
            }
            check_pages1_change = false;
        }

        private void comboBox38_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 18; i++)
            {
                ((ComboBox)this.Controls.Find("comboBox" + (i + 5), true)[0]).SelectedIndex = ((ComboBox)sender).SelectedIndex;
            }
        }

        private void textBox_limit_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (comboBox36.SelectedIndex < 0)
            {
                MessageBox.Show("Please select flashing mode");
                return;
            }
            Form_priview_new f_priview = new Form_priview_new();
            f_priview.mode = comboBox36.SelectedIndex >= 0 ? comboBox36.SelectedIndex : 0;

            f_priview.timer = 2;
            try { f_priview.timer = int.Parse(textBox4.Text); }
            catch { }

            f_priview.IsPattern = checkBox_pattern.Checked;
            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    int index = j * 6 + i + 1;

                    int c = ((ComboBox)this.Controls.Find("comboBox" + (index + 4), true)[0]).SelectedIndex;
                    string _color = inc.VMS_color_code[c].ToString();

                    f_priview.msg_pages[j, i].msg = _color + ((ComboBox)this.Controls.Find("comboBox" + (100 + index), true)[0]).Text;
                    f_priview.msg_pages[j, i].used = ((CheckBox)this.Controls.Find("checkBox" + index, true)[0]).Checked;

                }
            }
            f_priview.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox_source_vms.SelectedIndex >= 0)
            {
                try
                {
                    int m = fn_global.fn_vms_name2index(comboBox_VMSs.Text);
                    int i = fn_global.fn_vms_name2index(comboBox_source_vms.Text);
                    if (m == i)
                        return;
                    string path = fn_global.get_workdir("gdm" + inc._vms[i]._id);
                    string path1 = fn_global.get_workdir("gdm" + inc._vms[m]._id);
                    for (int j = 0; j < 6; j++)
                    {
                        string _name = path + "\\" + "gdm." + inc._vms[i]._id + "." + (j + 1) + ".txt";
                        string c_name = path1 + "\\" + "gdm." + inc._vms[m]._id + "." + (j + 1) + ".txt";
                        File.Copy(_name, c_name);
                    }
                }
                catch { }

                load_vms_config_rows();
            }
        }

        private void button40_Click(object sender, EventArgs e)
        {
            string _cmd = "<update_offset>";
            fn_global.fn_tcp_send(_cmd);
        }

        #region backup system
        private void button41_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process p1 = null;
            try
            {
                p1 = System.Diagnostics.Process.Start("backup\\LTA_PGS_BACKUP.exe");
                p1.WaitForInputIdle();
                System.Threading.Thread.Sleep(1000);
                //appWin1 = p1.MainWindowHandle;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error");
            }

            //SetParent(appWin1, this.Handle);
            //SetWindowLong(appWin1, GWL_STYLE, WS_VISIBLE);
        }
        #endregion

        private void button5_Click(object sender, EventArgs e)
        {
            string[] strs = new string[6];
            strs[0] = "^CA" + textBox_line1.Text;
            strs[1] = "^CA" + textBox_line2.Text;
            strs[2] = "^CA" + textBox_line3.Text;
            strs[3] = "^CA" + textBox5.Text;
            strs[4] = "^CA" + textBox3.Text;
            strs[5] = "^CA" + textBox2.Text;

            Form_priview_new f = new Form_priview_new();
            f.timer = 10;
            f.mode = 0;
            for (int i = 0; i < 6; i++)
            {
                f.msg_pages[0, i].msg = strs[i];
                f.msg_pages[0, i].used = true;
            }
            f.ShowDialog();
        }

        private void button18_Click(object sender, EventArgs e)
        {

            Form_priview_new f = new Form_priview_new();
            f.timer = 4;
            try
            {
                f.timer = int.Parse(textBox_timer.Text);
            }
            catch { }


            f.mode = comboBox_flash_mode.SelectedIndex >= 0 ? comboBox_flash_mode.SelectedIndex : 0;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int _index = j * 3 + i;

                    if (_vms_msg_str[_index]._rows_data.Text != "")
                    {
                        int n_c = _vms_msg_str[_index].name_color;
                        int l_c = _vms_msg_str[_index].lots_color;

                        string temp = inc.VMS_color_code[n_c - 1].ToString() + _vms_msg_str[_index]._rows_data.Text;
                        temp = temp.PadRight(17, ' ') + inc.VMS_color_code[l_c - 1].ToString() + "LOTS";

                        f.msg_pages[i, j].msg = temp;
                        f.msg_pages[i, j].used = true;
                    }
                }
            }
            f.ShowDialog();
        }
        #region area tabel control
        //dataGridView_area
        private void Initialize_area()
        {
            dataGridView_area.Columns.Add("id", "ID");
            dataGridView_area.Columns.Add("aid", "Area ID");
            dataGridView_area.Columns.Add("name", "Name");
            dataGridView_area.Columns.Add("map", "Map");
            dataGridView_area.Columns.Add("show", "Display");


            dataGridView_area.Columns["id"].Width = 90;
            dataGridView_area.Columns["aid"].Width = 90;
            dataGridView_area.Columns["name"].Width = 150;
            dataGridView_area.Columns["map"].Width = 190;
            dataGridView_area.Columns["show"].Width = 80;
        }

        private void load_area()
        {
            inc.db_control.SQL = "select * from tb_area order by fd_id";
            OdbcDataReader MyReader = inc.db_control.SQLExecuteReader();

            dataGridView_area.Rows.Clear();
            int i = 0;
            while (MyReader.Read())
            {
                dataGridView_area.Rows.Add();
                dataGridView_area.Rows[i].Cells["id"].Value = (Int64)MyReader["fd_id"];
                dataGridView_area.Rows[i].Cells["aid"].Value = (Int32)MyReader["fd_areaid"];
                dataGridView_area.Rows[i].Cells["name"].Value = (String)MyReader["fd_name"];
                dataGridView_area.Rows[i].Cells["map"].Value = (String)MyReader["fd_image"];
                dataGridView_area.Rows[i].Cells["show"].Value = (String)MyReader["fd_show"].ToString() == "1" ? "Yes" : "No";
                i++;
            }
            dataGridView_area.EndEdit();
            udpateareainfo();
            g_changemap = false;
        }


        private void udpateareainfo()
        {
            int i = -1;
            if (dataGridView_area.CurrentRow != null)
            {
                i = dataGridView_area.CurrentRow.Index;
            }

            if (i < 0)
                return;


            textBox19.Text = Tools.get_value(dataGridView_area.Rows[i].Cells["id"].Value);
            textBox18.Text = Tools.get_value(dataGridView_area.Rows[i].Cells["name"].Value);
            textBox17.Text = Tools.get_value(dataGridView_area.Rows[i].Cells["map"].Value);
            textBox15.Text = Tools.get_value(dataGridView_area.Rows[i].Cells["aid"].Value);

            checkBox22.Checked = (Tools.get_value(dataGridView_area.Rows[i].Cells["show"].Value)).ToString() == "Yes" ? true : false;
        }
        private void dataGridView_area_SelectionChanged(object sender, EventArgs e)
        {
            udpateareainfo();
        }

        private void button61_Click(object sender, EventArgs e)
        {
            if (!checkfiletype(textBox17.Text))
            {
                return;
            }
            if (g_changemap)
                if (!fn_global.fn_upload_file(textBox17.Text, "images", Path.GetFileName(textBox17.Text)))
                {
                    MessageBox.Show("Upload map file failed");
                    return;
                }
            try
            {
                dataGridView_area.Rows[dataGridView_area.CurrentRow.Index].Cells["name"].Value = textBox18.Text;
                dataGridView_area.Rows[dataGridView_area.CurrentRow.Index].Cells["map"].Value = textBox17.Text;
                dataGridView_area.Rows[dataGridView_area.CurrentRow.Index].Cells["aid"].Value = textBox15.Text;

                dataGridView_area.EndEdit();

                int i = dataGridView_area.CurrentRow.Index;

                string _id = Tools.get_value(dataGridView_area.Rows[i].Cells["id"].Value);
                string _aid = Tools.get_value(dataGridView_area.Rows[i].Cells["aid"].Value);

                string _name = Tools.get_value(dataGridView_area.Rows[i].Cells["name"].Value);

                string servername = Path.GetFileName(textBox17.Text);

                string show = checkBox22.Checked == true ? "1" : "0";

                inc.db_control.SQL = "update tb_area set fd_id=" + _id + ",fd_name='" + _name + "',fd_areaid=" + _aid + ",fd_image='" + servername + "' ,fd_show=" + show;
                inc.db_control.SQL += " where fd_id='" + _id + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {

                    MessageBox.Show("Update area successful");
                    fn_global.log_operateion((int)inc.LOGMSGCODE.ARU01, _name, 1);
                    load_area();
                    fn_global.load_area_table();
                }
                else
                    MessageBox.Show("Update message library failed");
            }
            catch (Exception e1)
            {
                MessageBox.Show("Update message library failed");
            }
        }

        private bool checkfiletype(string filename)
        {
            if (g_changemap)
            {
                if (!File.Exists(filename))
                {
                    MessageBox.Show(filename + " not exist,please select map file");
                    return false;
                }
                try
                {
                    string filetype = Path.GetExtension(filename);
                    if ((filetype != ".jpg") && (filetype != ".gif"))
                    {
                        MessageBox.Show("File type error, should be jpg or gif file");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
                return true;
            }
            else
                return true;
        }

        private void button62_Click(object sender, EventArgs e)
        {
            if (!checkfiletype(textBox17.Text))
            {
                return;
            }
            if (g_changemap)
                if (!fn_global.fn_upload_file(textBox17.Text, "images", Path.GetFileName(textBox17.Text)))
                {
                    MessageBox.Show("Upload map file failed");
                    return;
                }

            string servername = Path.GetFileName(textBox17.Text);
            string show = checkBox22.Checked == true ? "1" : "0";

            inc.db_control.SQL = "insert into tb_area (fd_name,fd_areaid,fd_x,fd_y,fd_image,fd_show)";
            inc.db_control.SQL += "values ('" + textBox18.Text + "'," + textBox15.Text + ",10,10,'" + servername + "'," + show + ")";
            if (inc.db_control.SQLExecuteReader() != null)
            {
                fn_global.log_operateion((int)inc.LOGMSGCODE.ARI01, textBox18.Text, 1);
                load_area();
                fn_global.load_area_table();
            }
        }

        private void button60_Click(object sender, EventArgs e)
        {

            if (MessageBox.Show("Delete the Area?", "Area", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                int i = dataGridView_area.CurrentRow.Index;
                string _id = Tools.get_value(dataGridView_area.Rows[i].Cells["id"].Value);

                inc.db_control.SQL = inc.db_control.delete_table("tb_area");
                inc.db_control.SQL += " where fd_id='" + _id + "'";
                if (inc.db_control.SQLExecuteReader() != null)
                {
                    fn_global.log_operateion((int)inc.LOGMSGCODE.ARD01, Tools.get_value(dataGridView_area.Rows[i].Cells["name"].Value), 1);
                    dataGridView_area.Rows.Remove(dataGridView_area.CurrentRow);

                    load_area();
                    fn_global.load_area_table();
                }
            }
        }
        private void textBox15_TextChanged(object sender, EventArgs e)
        {

        }

        private void label90_Click(object sender, EventArgs e)
        {

        }

        bool g_changemap = false;
        private void button63_Click(object sender, EventArgs e)
        {
            OpenFileDialog _loadfile = new OpenFileDialog();
            _loadfile.Filter = "*.gif|*.jpg";
            if (_loadfile.ShowDialog() == DialogResult.OK)
            {
                if (checkfiletype(_loadfile.FileName))
                {
                    g_changemap = true;
                    textBox17.Text = _loadfile.FileName;
                }
            }
        }

        #endregion
    }
}




