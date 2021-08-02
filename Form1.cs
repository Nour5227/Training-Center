using Oracle.DataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CrystalDecisions.Shared;

namespace Training_Center
{


    public partial class Form1 : Form
    {
        CrystalReport1 CR;
        GroupedByReport groupByReport;

        string[] manager = new string[2];
        string[] receptionist = new string[2];



        OracleConnection conn;
        OracleDataAdapter adapter, studentsAdapter;
        OracleCommandBuilder builder;
        DataSet ds, ds1;
        public Image buttonOutline;


        string constr = "Data source = orcl ; User id = scott; password=tiger;";
        string comstr = "select * from teacher;";
        string selectedCourse;
        List<string> courseNames = new List<string>();

        //noran//function to take the course name from the list and return its ID//
        private int Get_ID(string selected)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select COURSE_ID from COURSE where COURSE_NAME=:selected";
            cmd.Parameters.Add("selected", selected);
            cmd.CommandType = CommandType.Text;
            OracleDataReader dr = cmd.ExecuteReader();
            int COURSE_ID = -1;
            if (dr.Read())
            {
                COURSE_ID = Convert.ToInt32(dr["COURSE_ID"].ToString());
            }


            return COURSE_ID;
        }

        public Form1()
        {
            InitializeComponent();
            buttonOutline = Image.FromFile("buttonLayout.png");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CR = new CrystalReport1(); // summary report
            groupByReport = new GroupedByReport(); // group by report
            foreach (ParameterDiscreteValue course in groupByReport.ParameterFields[0].DefaultValues)
                comboBox1.Items.Add(course.Value); //add courses in combobox to search for a specific course (parameter)

            displayAllTeachers();
            displayAllStudents();
            crystalReportViewer1.ReportSource = CR;

            //ui 
            coursesBtnMngr.Image = null;
            coursesBtnMngr.ForeColor = Color.White;

            coursesBtnRecip.Image = null;
            coursesBtnRecip.ForeColor = Color.White;
            
            reportsBtn.Image = null;
            reportsBtn.ForeColor = Color.White;

           
            //

            conn = new OracleConnection(constr);
            conn.Open();
            updateListInReceptionist();
            updateListInManager();
            //


            //noran//load Teacher Ids in combobox
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "select TEACHER_SSN from TEACHER";
            cmd.CommandType = CommandType.Text;

            OracleDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                teacher_ID.Items.Add(dr[0]);
            }
            dr.Close();
           //noran //load student ids in combo box

            OracleCommand cm = new OracleCommand();
            cm.Connection = conn;
            cm.CommandText = "select STUDENT_SSN from STUDENT";
            cm.CommandType = CommandType.Text;

            OracleDataReader d = cm.ExecuteReader();
            while (d.Read())
            {
                student_ID.Items.Add(d[0]);
            }
            d.Close();


        }
        private void updateListInManager()
        {
            listBox2.Items.Clear();

            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SearchCourses";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add("course", textBox2.Text.ToLower());
            cmd.Parameters.Add("SearchOutput", OracleDbType.RefCursor, ParameterDirection.Output);
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                listBox2.Items.Add(reader[0]);
            }
            reader.Close();
        }
        private void updateListInReceptionist()
        {
            listBox1.Items.Clear();
            string columns = "{0, -30}\t{1, -30}";
            listBox1.Items.Add(string.Format(columns, "COURSE NAME", "TEACHER NAME"));

            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT COURSE_NAME , TEACHER_NAME FROM COURSE , TEACHER , TEACHES WHERE COURSE.COURSE_ID = TEACHES.COURSEID AND TEACHER.TEACHER_SSN = TEACHES.TEACHERSSN AND LOWER(COURSE_NAME) like  :NAME || '%' ";
            cmd.Parameters.Add("NAME", textBox4.Text.ToLower());
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string COURSE_NAME = reader["COURSE_NAME"].ToString();
                courseNames.Add(COURSE_NAME);
                string TEACHER_NAME = reader["TEACHER_NAME"].ToString();
                listBox1.Items.Add(string.Format(columns, COURSE_NAME, TEACHER_NAME));
            }
            reader.Close();
        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            updateListInReceptionist();

        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            
            updateListInManager();
        }

        // @seif hossam teachers in manager
        private void displayAllTeachers()
        {
            //string constr = "Data source=seif; User id=scott; password=tiger;";
            //string comstr = "select * from teacher";

            adapter = new OracleDataAdapter(comstr, constr);
            ds = new DataSet();
            adapter.Fill(ds);
            dataGridView1.DataSource = ds.Tables[0];
            builder = new OracleCommandBuilder(adapter);
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            if (textBox1.Text == "")
            {
                displayAllTeachers();
            }
            else
            {
                string comstr = @"select * from teacher where Lower(teacher_name) like '%' || :n || '%' ";

                adapter = new OracleDataAdapter(comstr, constr);
                adapter = new OracleDataAdapter(comstr, constr);
                adapter.SelectCommand.Parameters.Add("n", textBox1.Text.ToLower());
                ds = new DataSet();
                adapter.Fill(ds);
                dataGridView1.DataSource = ds.Tables[0];
            }
        }


        // @tasneem students in recip
        private void displayAllStudents()
        {
            string comstr = "select * from STUDENT";
            studentsAdapter = new OracleDataAdapter(comstr, constr);
            ds1 = new DataSet();
            studentsAdapter.Fill(ds1);
            dataGridView2.DataSource = ds1.Tables[0];

        }
        private void stdntSrshRciptTxtBox_TextChanged(object sender, EventArgs e)
        {
            if (stdntSrshRciptTxtBox.Text == "")
            {
                displayAllStudents();
            }
            else
            {
                string comstr = @"select * from STUDENT where Lower(STUDENT_Name) like '%' || :name || '%' ";

                studentsAdapter = new OracleDataAdapter(comstr, constr);
                studentsAdapter.SelectCommand.Parameters.Add("name", stdntSrshRciptTxtBox.Text.ToLower());
                ds1 = new DataSet();
                studentsAdapter.Fill(ds1);
                dataGridView2.DataSource = ds1.Tables[0];
            }
        }
        private void svStdntRcipBtn_Click(object sender, EventArgs e)
        {
            builder = new OracleCommandBuilder(studentsAdapter);
            studentsAdapter.Update(ds1.Tables[0]);
            MessageBox.Show("Student Added Successfully.");

        }



        // menu navigation buttons

        private void teachersbtn_Click(object sender, EventArgs e)
        {
            teachersPanel.BringToFront();

            teachersbtn.Image = buttonOutline;
            teachersbtn.ForeColor = Color.FromArgb(41, 171, 226);

            coursesBtnMngr.Image = null;
            coursesBtnMngr.ForeColor = Color.White;

            reportsBtn.Image = null;
            reportsBtn.ForeColor = Color.White;
        }

        private void studentsBtn_Click(object sender, EventArgs e)
        {
            studentsPanel.BringToFront();

            studentsBtn.Image = buttonOutline;
            studentsBtn.ForeColor = Color.FromArgb(41, 171, 226);

            coursesBtnRecip.Image = null;
            coursesBtnRecip.ForeColor = Color.White;
        }

        private void coursesBtnMngr_Click(object sender, EventArgs e)
        {
            coursesPanel.BringToFront();
            coursesBtnMngr.Image = buttonOutline;
            coursesBtnMngr.ForeColor = Color.FromArgb(41, 171, 226);

            teachersbtn.Image = null;
            teachersbtn.ForeColor = Color.White;

            reportsBtn.Image = null;
            reportsBtn.ForeColor = Color.White;
        }

        private void coursesBtnRecip_Click(object sender, EventArgs e)
        {
            coursesPanelRecip.BringToFront();

            coursesBtnRecip.Image = buttonOutline;
            coursesBtnRecip.ForeColor = Color.FromArgb(41, 171, 226);

            studentsBtn.Image = null;
            studentsBtn.ForeColor = Color.White;
        }

        private void reportsBtn_Click(object sender, EventArgs e)
        {
            Student_Summary.BringToFront();

            reportsBtn.Image = buttonOutline;
            reportsBtn.ForeColor = Color.FromArgb(41, 171, 226);

            teachersbtn.Image = null;
            teachersbtn.ForeColor = Color.White;

            coursesBtnMngr.Image = null;
            coursesBtnMngr.ForeColor = Color.White;

        }
        private void groupByCourseBtn_Click(object sender, EventArgs e)
        {
            Students_Grouped_By.BringToFront();
        }
        private void backBtn_Click(object sender, EventArgs e)
        {
            Student_Summary.BringToFront();
        }

        //login & out
        private void loginBtn_Click(object sender, EventArgs e)
        {
            manager[0] = "manager";
            manager[1] = "*123123";

            receptionist[0] = "receptionist";
            receptionist[1] = "321321";

            if (username.Text == "")
            {
                failedLoginMessage.Text = "Enter Username & Password";
                failedLoginMessage.Visible = true;
            }
            else if (username.Text == manager[0])
            {
                if (password.Text == manager[1])
                {
                    managerPanel.BringToFront();
                }
                else
                {
                    failedLoginMessage.Text = "Incorrect Username or Password";
                    failedLoginMessage.Visible = true;
                }
            }
            else if (username.Text == receptionist[0])
            {
                if (password.Text == receptionist[1])
                {
                    receptionistPanel.BringToFront();
                }
                else
                {
                    failedLoginMessage.Text = "Incorrect Username or Password";
                    failedLoginMessage.Visible = true;
                }
            }
            else
            {
                failedLoginMessage.Text = "Incorrect Username or Password";
                failedLoginMessage.Visible = true;
            }

        }
        private void password_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                loginBtn_Click(sender, e);
            }
        }
        private void logoutBtnMngr_Click(object sender, EventArgs e)
        {
            loginPanel.BringToFront();
        }
        private void username_TextChanged(object sender, EventArgs e)
        {
            failedLoginMessage.Visible = false;
        }

        //norann//check if the user select an item from the list or not and display the selected course in label ,in assign teachers and enroll students to courses//
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            if (listBox2.SelectedItem != null)
            {
                label4.Text = listBox2.SelectedItem.ToString();
                selectedCourse = label4.Text;
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = conn;
            if (listBox1.SelectedItem != null)
            {
                label6.Text = courseNames[listBox1.SelectedIndex - 1];
                selectedCourse = label6.Text;
            }
        }
        private void Save_Click(object sender, EventArgs e)
        {
            if (teacher_ID.SelectedItem == null || listBox2.SelectedItem == null)
            {
                MessageBox.Show("Failed . " +
                   "Select Teacher ID And Course Name.");
            }
            else
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;
                int courseID = Convert.ToInt32(Get_ID(selectedCourse));
                cmd.CommandText = "insert into TEACHES values (:ssn,:ID)";
                cmd.Parameters.Add("ssn", teacher_ID.Text);
                cmd.Parameters.Add("ID", Get_ID(selectedCourse));
                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Teacher Assigned To Course Successfully.");
                }
                catch
                {
                    MessageBox.Show("This Teacher Is Already Assigned To This Course.");

                }



            }
        }
        private void Save_btn_Click(object sender, EventArgs e)
        {
            if (student_ID.SelectedItem == null || listBox1.SelectedItem == null)
            {
                MessageBox.Show("Failed . " +
                    "Select Student ID And Course Name.");
            }
            else
            {
                OracleCommand cmd = new OracleCommand();
                cmd.Connection = conn;

                int courseID = Convert.ToInt32(Get_ID(selectedCourse));
                cmd.CommandText = "insert into ENROLLEDIN values (:ssn,:ID)";
                cmd.Parameters.Add("ssn", student_ID.Text);
                cmd.Parameters.Add("ID", Get_ID(selectedCourse));

                try
                {
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Student enrolled in course successfully.");
                }
                catch
                {
                    MessageBox.Show("This Student Is Already Enrolled In This Course.");

                }

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            groupByReport.SetParameterValue(0, comboBox1.Text);
            crystalReportViewer2.ReportSource = groupByReport;
        }








        //noran//displaying teacher information when choosing the id from combobox//
        private void teacher_ID_SelectedIndexChanged(object sender, EventArgs e)
        {
            OracleCommand c = new OracleCommand();
            c.Connection = conn;
            c.CommandText = "select TEACHER_SSN,TEACHER_AGE,TEACHER_NAME,TEACHER_PHONE_NUMBER from TEACHER where TEACHER_SSN=:id";
            c.CommandType = CommandType.Text;
            c.Parameters.Add("id", teacher_ID.SelectedItem.ToString());
            OracleDataReader dr = c.ExecuteReader();
            if (dr.Read())
            {
                teacher_name_txtbox.Text = dr[2].ToString();
                teacher_age_txtbox.Text = dr[1].ToString();
                teacher_phoneNumber_txtbox.Text = dr[3].ToString();

            }
            dr.Close();

        }
        //noran//displaying student information when choosing the id from combobox //
        private void student_ID_SelectedIndexChanged(object sender, EventArgs e)
        {
            OracleCommand c = new OracleCommand();
            c.Connection = conn;
            c.CommandText = "select STUDENT_SSN,STUDENT_AGE,STUDENT_NAME,STUDENT_PHONE_NUMBER from STUDENT where STUDENT_SSN=:id";
            c.CommandType = CommandType.Text;
            c.Parameters.Add("id", student_ID.SelectedItem.ToString());
            OracleDataReader dr = c.ExecuteReader();
            if (dr.Read())
            {
                student_name_txtbox.Text = dr[2].ToString();
                student_age_txtbox.Text = dr[1].ToString();
                student_phoneNumber_txtbox.Text = dr[3].ToString();

            }
            dr.Close();
        }

    }
}

