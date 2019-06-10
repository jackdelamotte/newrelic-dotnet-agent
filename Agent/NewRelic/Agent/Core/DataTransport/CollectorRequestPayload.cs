namespace NewRelic.Agent.Core.DataTransport
{
	public class CollectorRequestPayload
	{
		public bool IsCompressed { get; set; }
		public string CompressionType { get; }
		public byte[] Data { get; }

		public CollectorRequestPayload(bool isCompressed, string compressionType, byte[] data)
		{
			IsCompressed = isCompressed;
			CompressionType = compressionType;
			Data = data;
		}
	}
}
