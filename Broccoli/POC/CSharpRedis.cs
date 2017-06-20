using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broccoli.POC
{
    public class CSharpRedis
    {
        #region REDIS FAILED PERFORMANCE TEST
        //* REDIS TEST FAILED, memory doesnt pass the test
        public void ConnectRedis()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost:6650");
        }

        public void testRedis(ConnectionMultiplexer redis)
        {
            redis = ConnectionMultiplexer.Connect("localhost:6650");

            ISubscriber sub = redis.GetSubscriber();
        }
        #endregion
    }
}
