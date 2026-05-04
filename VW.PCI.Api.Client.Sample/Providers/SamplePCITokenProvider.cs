using System;
using System.Collections.Generic;
using VW.Api.RestClient.Models;
using VW.PCI.Api.Client;
using VW.PCI.Api.Client.Models.Common;
using VW.PCI.Api.Client.Models.Requests;

namespace VW.PCI.Api.Client.Sample.Providers
{
    /// <summary>
    /// Ví dụ implement PCIDbTokenProvider dùng in-memory dictionary giả lập DB.
    ///
    /// Trong project thực tế, thay phần "giả lập DB" bằng:
    ///   - ADO.NET  : SqlCommand + stored procedure
    ///   - Dapper   : connection.QueryFirstOrDefault<PCITokenInfo>(sql)
    ///   - EF Core  : _dbContext.PCITokens.FirstOrDefault()
    /// </summary>
    public class SamplePCITokenProvider : PCIDbTokenProvider
    {
        // -----------------------------------------------------------------------
        // Giả lập bảng DB: trong thực tế đây là bảng tbl_PCI_Token hoặc tương tự
        // -----------------------------------------------------------------------
        private static PCITokenInfo _dbRow = null;

        public SamplePCITokenProvider(AuthTokenRequest credentials, ILogger logger)
            : base(credentials, logger) { }

        // -----------------------------------------------------------------------
        // Implement 3 abstract methods — đây là phần bạn viết lại theo DB thực tế
        // -----------------------------------------------------------------------

        protected override PCITokenInfo GetTokenFromDB()
        {
            // Thực tế: SELECT TOP 1 AccessToken, TokenType, ExpireAt FROM tbl_PCI_Token
            Logger.Debug("SamplePCITokenProvider: GetTokenFromDB()");
            return _dbRow;
        }

        protected override void SaveTokenToDB(PCITokenInfo token)
        {
            // Thực tế: MERGE INTO tbl_PCI_Token (upsert AccessToken, TokenType, ExpireAt)
            Logger.Debug($"SamplePCITokenProvider: SaveTokenToDB() — ExpireAt={token.ExpireAt:O}");
            _dbRow = token;
        }
    }
}
