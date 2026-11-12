using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace LibraryManagement.DataAccess.Context
{
    public interface IDbContext
    {
        IDbConnection CreateConnection();
    }
}
