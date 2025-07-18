﻿using System.Data;
using Dapper;
using TenentManagement.Models.Payment;
using TenentManagement.Services.Database;

namespace TenentManagement.Services.Payment
{
    public class PaymentInvoiceService
    {
        private readonly DatabaseConnection _dbConnection;
        public PaymentInvoiceService(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public PaymentInvoiceModel? GetPaymentInvoiceById(int id)
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'G');
            parameters.Add("@ID", id);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PaymentInvoiceModel>("SP_PAYMENTINVOICES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
        public int CreatePaymentInvoice(PaymentInvoiceModel payment)
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'I');
            parameters.Add("@RENTERID", payment.RenterId);
            parameters.Add("@UNITID", payment.UnitId);
            parameters.Add("@FROMMONTH", payment.FromMonth);
            parameters.Add("@TOMONTH", payment.ToMonth);
            parameters.Add("@DUEDATE", payment.DueDate);
            parameters.Add("@AMOUNTDUE", payment.AmountDue);
            parameters.Add("@OWNERID", payment.OwnerId);
            parameters.Add("@PAYMENTMETHODID", payment.PaymentMethodId);
            parameters.Add("@REMARK", payment.Remark);
            parameters.Add("@ISVERIFIED", payment.IsVerified);
            parameters.Add("@STATUSID", payment.StatusId);
            parameters.Add("@INSERTEDID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            connection.Open();
            var result = connection.Execute("SP_PAYMENTINVOICES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return parameters.Get<int>("@INSERTEDID");
        }

        public List<PaymentInvoiceModel> GetAllInvoiceOfUnit(int unitId)
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'A');
            parameters.Add("@UNITID", unitId);
            connection.Open();
            var result = connection.Query<PaymentInvoiceModel>("SP_PAYMENTINVOICES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }

        public int DeletePaymentInvoice(int id)
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'D');
            parameters.Add("@ID", id);
            connection.Open();
            var result = connection.Execute("SP_PAYMENTINVOICES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }

        public List<PaymentMethodModel> GetAllPaymentMethod()
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            connection.Open();
            var result = connection.Query<PaymentMethodModel>("SP_PAYMENTMETHOD", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }
        public List<PaymentStatusModel> GetAllPaymentStatus()
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'R');
            connection.Open();
            var result = connection.Query<PaymentStatusModel>("SP_PAYMENTSTATUS", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return [.. result];
        }
        public PaymentInvoiceModel? GetLatestMonth(int unitId)
        {
            var connection = _dbConnection.GetConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@FLAG", 'L');
            parameters.Add("@UNITID", unitId);
            connection.Open();
            var result = connection.QueryFirstOrDefault<PaymentInvoiceModel>("SP_PAYMENTINVOICES", parameters, commandType: System.Data.CommandType.StoredProcedure);
            connection.Close();
            return result;
        }
    }
}
