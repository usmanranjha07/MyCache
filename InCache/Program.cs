using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Threading;


namespace InCache
{
    class Program
    {


        private static IDictionary<int, data> myCache = new Dictionary<int, data>();
        private static Queue<timestamp> myCacheQueue = new Queue<timestamp>();

         struct data
        {
            public string fname;
            public double balance;
        }

         struct timestamp
         {
             public int account;
             public DateTime cacheTime;
         }

        public static void myPurgefunct()
        {

            timestamp acc_t;
            do
            {
                if(myCacheQueue.Count>0 && DateTime.Now > myCacheQueue.Peek().cacheTime.AddMinutes(2))
                {

                    acc_t=myCacheQueue.Dequeue();
                    myCache.Remove(acc_t.account);
                    Thread.Sleep(1000);
                }


            } while (true);
        
        }

        static void Main(string[] args)
        {

            

            data temp;
            timestamp time;

            Thread backgroundThread = new Thread(new ThreadStart(Program.myPurgefunct));
            backgroundThread.IsBackground = true;
            backgroundThread.Start();  

            SqlConnection conn = new SqlConnection(@"Data Source=(LocalDB)\v11.0;AttachDbFilename=E:\InCache\InCache\Database1.mdf;Integrated Security=True");
            int op = 0;
            int acc = 0;
            string fname = "";
            double bal = 0;

            Console.WriteLine("press 1 to get balance");
            Console.WriteLine("press 2 to set balance");
            Console.WriteLine("press 0 to exit");

            op = Convert.ToInt16(Console.ReadLine());

            do
            {


                if (op == 1)
                {

                    Console.WriteLine("Enter Account #");
                    acc = Convert.ToInt32(Console.ReadLine());

                   
                    if (myCache.ContainsKey(acc))
                    {

                        Console.WriteLine("Full Name: {0}", myCache[acc].fname);

                        bal = myCache[acc].balance;

                        Console.WriteLine("Balance in (USDT): {0}", bal);
                        Console.WriteLine("Balance in (ETH): {0}", bal / 2300);
                        Console.WriteLine("Balance in (BTC): {0}", bal / 48000);

                    }
                    else
                    {
                        string sql = "select * from Account where Account_ID=" + acc;
                        SqlCommand sqlcmd = new SqlCommand(sql, conn);

                        conn.Open();
                        SqlDataReader reader = sqlcmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine("Full Name: {0}", reader["Name"].ToString());
                                temp.fname = reader["Name"].ToString();
                                
                                bal = Convert.ToDouble(reader["Balance"].ToString());
                                temp.balance = bal;
                                
                                Console.WriteLine("Balance in (USDT): {0}", bal);
                                Console.WriteLine("Balance in (ETH): {0}", bal / 2300);
                                Console.WriteLine("Balance in (BTC): {0}", bal / 48000);

                                myCache.Add(acc, temp);
                                time.account = acc;
                                time.cacheTime = DateTime.Now;
                                myCacheQueue.Enqueue(time);

                            }
                            reader.Close();
                            conn.Close();

                        }
                        else
                        {
                            Console.WriteLine("Invalid acc #");
                        }


                    }

                }
                else if (op == 2)
                {
                    Console.WriteLine("Enter Account #");
                    acc = Convert.ToInt32(Console.ReadLine());

                    Console.WriteLine("Enter Full Name:");
                    fname = Console.ReadLine();

                    int op2 = 0;
                    Console.WriteLine("press 1 to set Balance in (USDT)");
                    Console.WriteLine("press 2 to set Balance in (ETH)");
                    Console.WriteLine("press 3 to set Balance in (BTC)");
                    op2 = Convert.ToInt16(Console.ReadLine());

                    if (op2 == 1)
                    {
                        Console.WriteLine("Enter amount");
                        bal = Convert.ToDouble(Console.ReadLine());
                    }
                    else if (op2 == 2)
                    {
                        Console.WriteLine("Enter amount");
                        bal = Convert.ToDouble(Console.ReadLine());
                        bal = bal * 2300;
                    }
                    else if (op2 == 3)
                    {
                        Console.WriteLine("Enter amount");
                        bal = Convert.ToDouble(Console.ReadLine());
                        bal = bal * 48000;
                    }
                    else
                    {
                        bal = 0;
                    }

                    if (!myCache.ContainsKey(acc))
                    {
                        temp.balance = bal;
                        temp.fname = fname;

                        myCache.Add(acc, temp);
                        time.account = acc;
                        time.cacheTime = DateTime.Now;
                        myCacheQueue.Enqueue(time);

                        
                    }
                    string sql = "select * from Account where Account_ID=" + acc;
                    SqlCommand sqlcmd = new SqlCommand(sql, conn);

                    conn.Open();
                    SqlDataReader reader = sqlcmd.ExecuteReader();

                    if (reader.HasRows)
                    {

                        reader.Close();

                        SqlCommand myCommand = new SqlCommand("UPDATE Account SET Name = @fname, Balance=@bal WHERE Account_ID = @acc", conn);

                        myCommand.Parameters.AddWithValue("@fname", fname);
                        myCommand.Parameters.AddWithValue("@bal", bal);
                        myCommand.Parameters.AddWithValue("@acc", acc);
                        myCommand.ExecuteNonQuery();

                        conn.Close();
                    }
                    else
                    {
                        reader.Close();

                        SqlCommand myCommand = new SqlCommand("INSERT INTO Account (Account_ID, Name, Balance) VALUES (@acc,@fname,@bal);", conn);

                        myCommand.Parameters.AddWithValue("@fname", fname);
                        myCommand.Parameters.AddWithValue("@bal", bal);
                        myCommand.Parameters.AddWithValue("@acc", acc);
                        myCommand.ExecuteNonQuery();

                        conn.Close();
                    }
                }
                else if (op == 0)
                {
                    return;
                }


                Console.WriteLine("press 1 to get balance");
                Console.WriteLine("press 2 to set balance");
                Console.WriteLine("press 0 to exit");

                op = Convert.ToInt16(Console.ReadLine());

            } while (op != 0);


            //Console.ReadKey();
        }
    }
}
