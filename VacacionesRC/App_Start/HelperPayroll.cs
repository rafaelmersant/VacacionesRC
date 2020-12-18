using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace VacacionesRC.App_Start
{
    public class HelperPayroll
    {
        public struct PayrollDetailRows
        {
            public string ceingdeduc;
            public string cetipotrans;
            public string cedesctran;
            public decimal cevaltrans;
            public decimal cebalaactu;
        }

        public struct PayrollDetailHeader
        {
            public string cecodire;
            public string cetipopago;
            public string cedescpago;
            public string ceciclopag;
            public string cecodemple;
            public string cenomemple;
            public string cenomdepto;
            public string cenomcargo;
            public string cecorreoel;
            public string cecuebanco;
            public string cenusegsoc;
            public string cenumcedul;
            public string cedesdirec;
            public string cedescfpag;
            public string cedesctcue;
            public string cetiponom;
            public decimal incomeTotal;
            public decimal discountTotal;
            public decimal balanceTotal;
            public decimal total;
            public List<PayrollDetailRows> detail;
        }

        public static PayrollDetailHeader GetPayrollDetail(DataSet data)
        {
            PayrollDetailHeader payrollDetail = new PayrollDetailHeader();

            if (data.Tables.Count > 0 && data.Tables[0].Rows.Count > 0)
            {
                decimal amount = 0;

                payrollDetail.discountTotal = 0;
                payrollDetail.incomeTotal = 0;
                payrollDetail.balanceTotal = 0;
                payrollDetail.total = 0;

                payrollDetail.cecodire = data.Tables[0].Rows[0].ItemArray[0].ToString();
                payrollDetail.cetipopago = data.Tables[0].Rows[0].ItemArray[1].ToString();
                payrollDetail.cedescpago = data.Tables[0].Rows[0].ItemArray[2].ToString();
                payrollDetail.ceciclopag = data.Tables[0].Rows[0].ItemArray[7].ToString();
                payrollDetail.cecodemple = data.Tables[0].Rows[0].ItemArray[8].ToString();
                payrollDetail.cenomemple = data.Tables[0].Rows[0].ItemArray[9].ToString();
                payrollDetail.cenomdepto = data.Tables[0].Rows[0].ItemArray[10].ToString();
                payrollDetail.cenomcargo = data.Tables[0].Rows[0].ItemArray[11].ToString();
                payrollDetail.cecorreoel = data.Tables[0].Rows[0].ItemArray[12].ToString();
                payrollDetail.cecuebanco = data.Tables[0].Rows[0].ItemArray[13].ToString();
                payrollDetail.cenusegsoc = data.Tables[0].Rows[0].ItemArray[14].ToString();
                payrollDetail.cenumcedul = data.Tables[0].Rows[0].ItemArray[15].ToString();
                payrollDetail.cedesdirec = data.Tables[0].Rows[0].ItemArray[16].ToString();
                payrollDetail.cedescfpag = data.Tables[0].Rows[0].ItemArray[17].ToString();
                payrollDetail.cedesctcue = data.Tables[0].Rows[0].ItemArray[18].ToString();
                payrollDetail.cetiponom = data.Tables[0].Rows[0].ItemArray[20].ToString();

                payrollDetail.detail = new List<PayrollDetailRows>();

                foreach (DataRow row in data.Tables[0].Rows)
                {
                    amount = decimal.Parse(row.ItemArray[6].ToString());

                    decimal balance = decimal.Parse(row.ItemArray[19].ToString());

                    if (row.ItemArray[3].ToString() == "I") payrollDetail.incomeTotal += amount;
                    if (row.ItemArray[3].ToString() != "I") payrollDetail.discountTotal += amount;
                    if (row.ItemArray[3].ToString() == "D" && balance > 0) payrollDetail.balanceTotal += balance;

                    //contactenate cantidad to description
                    string _cedesctran = row.ItemArray[5].ToString();
                    if (!string.IsNullOrEmpty(row.ItemArray[21].ToString()))
                    {
                        try
                        {
                            decimal hours = decimal.Parse(row.ItemArray[21].ToString());
                            if (hours > 0)
                                _cedesctran += $" ({row.ItemArray[21].ToString()} Horas)";
                        }
                        catch (Exception ex)
                        {
                            Helper.SendException(ex);
                        }
                    }

                    payrollDetail.detail.Add(new PayrollDetailRows
                    {
                        ceingdeduc = row.ItemArray[3].ToString(),
                        cetipotrans = row.ItemArray[4].ToString(),
                        cedesctran = _cedesctran,
                        cevaltrans = amount,
                        cebalaactu = decimal.Parse(row.ItemArray[19].ToString())
                    });
                }

                payrollDetail.total = payrollDetail.incomeTotal - payrollDetail.discountTotal;
            }
            return payrollDetail;
        }

        public static string GetPayrollPeriodByAdmissionDate(DateTime admissionDate)
        {
            int month = admissionDate.Month;
            int day = admissionDate.Day;
            int year = DateTime.Today.Year;
            int oldYear = year - 1;

            if (month == 1 && day > 15)
                return string.Format("{0}0101", year);
            if (month == 1 && day < 15)
                return string.Format("{0}1224", oldYear);

            if (month == 2 && day > 15)
                return string.Format("{0}0203", year);
            if (month == 2 && day < 15)
                return string.Format("{0}0102", year);

            if (month == 3 && day > 15)
                return string.Format("{0}0305", year);
            if (month == 3 && day < 15)
                return string.Format("{0}0204", year);

            if (month == 4 && day > 15)
                return string.Format("{0}0407", year);
            if (month == 4 && day < 15)
                return string.Format("{0}0306", year);

            if (month == 5 && day > 15)
                return string.Format("{0}0509", year);
            if (month == 5 && day < 15)
                return string.Format("{0}0408", year);

            if (month == 6 && day > 15)
                return string.Format("{0}0611", year);
            if (month == 6 && day < 15)
                return string.Format("{0}0510", year);

            if (month == 7 && day > 15)
                return string.Format("{0}0713", year);
            if (month == 7 && day < 15)
                return string.Format("{0}0612", year);

            if (month == 8 && day > 15)
                return string.Format("{0}0815", year);
            if (month == 8 && day < 15)
                return string.Format("{0}0714", year);

            if (month == 9 && day > 15)
                return string.Format("{0}0917", year);
            if (month == 9 && day < 15)
                return string.Format("{0}0816", year);

            if (month == 10 && day > 15)
                return string.Format("{0}1019", year);
            if (month == 10 && day < 15)
                return string.Format("{0}0918", year);

            if (month == 11 && day > 15)
                return string.Format("{0}1121", year);
            if (month == 11 && day < 15)
                return string.Format("{0}1020", year);

            if (month == 12 && day > 15)
                return string.Format("{0}1223", year);
            if (month == 12 && day < 15)
                return string.Format("{0}1122", year);

            return "";
        }

        public static DateTime? GetPayrollPeriodByRenovationDate(DateTime admissionDate)
        {
            int month = admissionDate.Month;
            int day = admissionDate.Day;
            int year = DateTime.Today.Year;
            int oldYear = year - 1;

            if (month == 1 && day > 15)
                return DateTime.Parse(string.Format("{0}-01-01", year));
            if (month == 1 && day < 15)
                return DateTime.Parse(string.Format("{0}-12-15", oldYear));
            
            if (month == 2 && day > 15)
                return DateTime.Parse(string.Format("{0}-02-01", year));
            if (month == 2 && day < 15)
                return DateTime.Parse(string.Format("{0}-01-15", year));

            if (month == 3 && day > 15)
                return DateTime.Parse(string.Format("{0}-03-01", year));
            if (month == 3 && day < 15)
                return DateTime.Parse(string.Format("{0}-02-15", year));

            if (month == 4 && day > 15)
                return DateTime.Parse(string.Format("{0}-04-01", year));
            if (month == 4 && day < 15)
                return DateTime.Parse(string.Format("{0}-03-15", year));

            if (month == 5 && day > 15)
                return DateTime.Parse(string.Format("{0}-05-01", year));
            if (month == 5 && day < 15)
                return DateTime.Parse(string.Format("{0}-04-01", year));

            if (month == 6 && day > 15)
                return DateTime.Parse(string.Format("{0}-06-01", year));
            if (month == 6 && day < 15)
                return DateTime.Parse(string.Format("{0}-05-15", year));

            if (month == 7 && day > 15)
                return DateTime.Parse(string.Format("{0}-07-01", year));
            if (month == 7 && day < 15)
                return DateTime.Parse(string.Format("{0}-06-15", year));

            if (month == 8 && day > 15)
                return DateTime.Parse(string.Format("{0}-08-01", year));
            if (month == 8 && day < 15)
                return DateTime.Parse(string.Format("{0}-07-15", year));

            if (month == 9 && day > 15)
                return DateTime.Parse(string.Format("{0}-09-01", year));
            if (month == 9 && day < 15)
                return DateTime.Parse(string.Format("{0}-08-15", year));

            if (month == 10 && day > 15)
                return DateTime.Parse(string.Format("{0}-10-01", year));
            if (month == 10 && day < 15)
                return DateTime.Parse(string.Format("{0}-09-15", year));

            if (month == 11 && day > 15)
                return DateTime.Parse(string.Format("{0}-11-01", year));
            if (month == 11 && day < 15)
                return DateTime.Parse(string.Format("{0}-10-15", year));

            if (month == 12 && day > 15)
                return DateTime.Parse(string.Format("{0}-12-01", year));
            if (month == 12 && day < 15)
                return DateTime.Parse(string.Format("{0}-11-15", year));

            return null;
        }
    }
}