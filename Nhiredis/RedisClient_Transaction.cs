using System;
using System.Collections.Generic;
using System.Threading;

namespace Nhiredis
{
    public partial class RedisClient
    {
        public delegate void TransactionFunction();

        public void Transaction(
            string name,
            TimeSpan delayBeforeRetry,
            int maxRetries,
            TransactionFunction preMulti,
            TransactionFunction postMulti
            )
        {
            int tryCount = 0;
            while (true)
            {
                preMulti();
                RedisCommand("MULTI");
                postMulti();
                var result = RedisCommand<List<string>>("EXEC");

                if (result != null)
                {
                    break;
                }

                tryCount += 1;
                if (tryCount > maxRetries)
                {
                    throw new NhiredisException("Transaction failed: " + name);
                }
                Thread.Sleep(delayBeforeRetry);
            }
        }

    }
}
