using System;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SingleSand.Utils;

namespace SingleSand.Amq.DataModel
{
    /// <summary>
    /// Implements asynchronous serialization on top of synchronous serializer.
    /// Note: It is based on data length value supplied by a stream.
    /// </summary>
    public static class AsyncSerializer
    {
        private const int MaxDataLength = 1024 * 256;

        public static async Task<T> DeserializeAsync<T>(this ISerializer synchronousSerializer, Stream input, CancellationToken cancellationToken)
        {
            //step 1: read data lenght field
            const int lengthFieldLength = 4;
            var dataLengthBuffer = new byte[lengthFieldLength];
            if (await input.ReadAsync(dataLengthBuffer, 0, lengthFieldLength, cancellationToken)
                    .CancelWith(() => { throw new TaskCanceledException(); }, cancellationToken)
                    != lengthFieldLength
                && !cancellationToken.IsCancellationRequested)
                throw new SerializationException("Input stream does not contain message length value");

            cancellationToken.ThrowIfCancellationRequested();

            var dataLength = BitConverter.ToInt32(dataLengthBuffer, 0);
            if (dataLength <= 0 || dataLength > MaxDataLength)
                throw new SerializationException(string.Format("Data length is invalid: {0}", dataLength));

            //step 2: read rest data of defined length
            var buffer = new byte[dataLength];
            if (await input.ReadAsync(buffer, 0, dataLength, cancellationToken)
                    .CancelWith(() => { throw new TaskCanceledException(); }, cancellationToken)
                    != dataLength
                && !cancellationToken.IsCancellationRequested)
                throw new SerializationException("Input stream does not have enough data");

            cancellationToken.ThrowIfCancellationRequested();

            //step 3: deserialize
            return synchronousSerializer.Deserialize<T>(buffer);
        }

        public static async Task SerializeAsync<T>(this ISerializer synchronousSerializer, T instance, Stream output,
                                                   CancellationToken cancellationToken)
        {
            //step 1: serialize
            var buffer = synchronousSerializer.Serialize(instance);

            //step 2: write data lenght field
            var dataSizeBuffer = BitConverter.GetBytes(buffer.Length);
            await output.WriteAsync(dataSizeBuffer, 0, dataSizeBuffer.Length)
                .CancelWith(() => { throw new TaskCanceledException(); }, cancellationToken);

            //step 3: write rest data
            await output.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}