using System;
using System.Collections.Generic;
using System.Threading;

namespace Nhiredis
{
    public partial class RedisClient
    {
        public delegate void TransactionFunction();

        public enum TransactionRetryType
        {
            Randomize,
            Predictable
        }

        public void Transaction(
            string name,
            TimeSpan delayBeforeRetry,
            int maxRetries,
            TransactionRetryType retryType,
            TransactionFunction preMulti,
            TransactionFunction postMulti
            )
        {
            Random r = null;

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

                if (retryType == TransactionRetryType.Predictable)
                {
                    Thread.Sleep(delayBeforeRetry);
                }
                else
                {
                    if (r == null)
                    {
                        r = new Random(DateTime.Now.Millisecond);
                    }
                    double maxDelay = delayBeforeRetry.TotalMilliseconds;
                    double delay = maxDelay * (r.NextDouble() * 0.9 + 0.1);
                    Thread.Sleep(TimeSpan.FromMilliseconds(delay));
                }
            }
        }

    }
}
