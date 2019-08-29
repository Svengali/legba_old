using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

using Optional;

//using static net.Views;

//using EntityId = lib.Id<ent.Id>;


namespace ent
{




	public enum Local
	{

	}

	public enum Remote
	{

	}

	public enum Gateway
	{

	}

	public enum Client
	{

	}



	public partial interface IEntity<T>
	{
		EntityId Id { get; }
		Option<T> Com<T>() where T : class;
	}



	[gen.NetView( new[] { typeof( net.View<Local> ), typeof( net.View<Remote> ) } )]
	public partial class Entity<T> : IEntity<T>, net.Versioned
	{
		public EntityId Id => m_id;

		public Option<T> Com<T>() where T : class
		{
			return Option.None<T>();
		}




		EntityId m_id;

		//ImmutableDictionary<string, Component> m_coms;




	}




	[gen.NetView( new[] { typeof( net.View<Local> ), typeof( net.View<Remote> ) } )]
	public partial class Component<T> : net.Versioned
	{
	}




	/*
	public partial interface IComponent
	{

	}


	[gen.NetView( new[] { typeof( net.View<Local> ), typeof( net.View<Remote> ), typeof( net.View<Gateway> ), typeof( net.View<Client> ) } )]
	public partial class Component : IComponent
	{
	}


	public partial class ComTest : Component
	{

	}



	public partial interface IComHealth
	{
		float Health { get; }
	}

	public partial interface IComHealth_Local : IComHealth
	{
		float Health { get; set; }
	}


	public partial interface IComHealth_Remote : IComHealth
	{
	}


	// [gen.NetView( Rule )]
	// [gen.NetDist( Rule, Edge ), gen.NetDist( Edge, Client )]
	public partial class ComHealth : Component, IComHealth
	{
		float m_health;
	}

	public partial class ComHealth_Local : ComHealth, IComHealth_Local
	{
		float IComHealth_Local.Health { get => m_health; set => m_health = value; }
	}

	public partial class ComHealth_Remote : ComHealth, IComHealth_Remote
	{
	}





	// [gen.NetView( Rule )]
	public partial class ComPhysical : Component
	{
		//math.Vec3 m_pos;
	}


	[gen.NetView( new[] { typeof( net.View<Local> ) } )]
	public partial class ComAdmin : Component
	{
	}


	[gen.NetView( new[] { typeof( net.View<Local> ) } )]
	public partial class ComTarget : Component
	{
		
	}

	*/









	public class EntityTest
	{
		public EntityTest()
		{
			//var ent_1 = new Entity();
		}
	}


}
