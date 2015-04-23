using System;

namespace HelloFacets
{
	public class Location : IEquatable<Location>
	{
		public double Lat { get; set; }
		public double Lon { get; set; }

		#region Equality
		public bool Equals(Location other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Lat.Equals(other.Lat) && Lon.Equals(other.Lon);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Location) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (Lat.GetHashCode()*397) ^ Lon.GetHashCode();
			}
		}

		public static bool operator ==(Location left, Location right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Location left, Location right)
		{
			return !Equals(left, right);
		}
		#endregion    
	}
}