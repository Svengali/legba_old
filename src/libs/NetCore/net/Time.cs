using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace net
{


public class Time
{
	static public Time Cur => s_cur;

	static public void AppStartup()
	{
		s_cur = new Time();
	}

	static public void AppShutdown()
	{		
	}


	public Time()
	{
		m_seconds = 0.0;
	}

	public double Now => m_seconds;







	double m_seconds;




	static Time s_cur;
}



}
