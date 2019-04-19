using System;
using System.Collections.Generic;
using System.Text;

namespace svc
{



[Serializable]
public class PhysicsCfg : lib.Config
{

}

public partial class Physics : ServiceWithConfig<PhysicsCfg>
{
	public Physics( lib.Token _id, res.Ref<PhysicsCfg> _cfg )
		: base( _id, _cfg )
	{
	}
}








}
