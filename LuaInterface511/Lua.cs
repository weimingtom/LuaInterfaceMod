using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Security;

namespace LuaInterface511
{
	public enum LuaTypes 
	{
		LUA_TNONE = -1,
		LUA_TNIL = 0,
		LUA_TNUMBER = 3,
		LUA_TSTRING = 4,
		LUA_TBOOLEAN = 1,
		LUA_TTABLE = 5,
		LUA_TFUNCTION = 6,
		LUA_TUSERDATA = 7,
		LUA_TLIGHTUSERDATA = 2
	}

	public enum LuaGCOptions
	{
		LUA_GCSTOP = 0,
		LUA_GCRESTART = 1,
		LUA_GCCOLLECT = 2,
		LUA_GCCOUNT = 3,
		LUA_GCCOUNTB = 4,
		LUA_GCSTEP = 5,
		LUA_GCSETPAUSE = 6,
		LUA_GCSETSTEPMUL = 7,
	}

	sealed class LuaIndexes 
	{
		public static int LUA_REGISTRYINDEX = -10000;
		public static int LUA_ENVIRONINDEX = -10001;
		public static int LUA_GLOBALSINDEX = -10002;
	}

	[StructLayout( LayoutKind.Sequential )]
	public struct ReaderInfo
	{
		public String chunkData;
		public bool finished;
	}

	public delegate int LuaCSFunction(IntPtr luaState);

	public delegate string LuaChunkReader(IntPtr luaState,ref ReaderInfo data,ref uint size);

    public delegate int LuaFunctionCallback(IntPtr luaState);

	public class Lua 
	{
        const string BASEPATH = "";
		const string LUADLL = BASEPATH + "lua511.dll";
		const string LUALIBDLL = BASEPATH + "lua511.dll";

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern string lua_typename(IntPtr luaState, LuaTypes type);
		
		public static string luaL_typename(IntPtr luaState, int stackPos)
		{
			return Lua.lua_typename(luaState, Lua.lua_type(luaState, stackPos));
		}

		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_error(IntPtr luaState, string message);
		
		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern string luaL_gsub(IntPtr luaState, string str, string pattern, string replacement);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_getfenv(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_isfunction(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_islightuserdata(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_istable(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_isuserdata(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_lessthan(IntPtr luaState, int stackPos1, int stackPos2);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_rawequal(IntPtr luaState, int stackPos1, int stackPos2);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_setfenv(IntPtr luaState, int stackPos);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_setfield(IntPtr luaState, int stackPos, string name);
		
		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_callmeta(IntPtr luaState, int stackPos, string name);

		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_newstate();

		public static IntPtr lua_open()
		{
			return Lua.luaL_newstate();
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_close(IntPtr luaState);

		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_openlibs(IntPtr luaState);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_objlen(IntPtr luaState, int stackPos);

		public static int lua_strlen(IntPtr luaState, int stackPos)
		{
			return lua_objlen(luaState, stackPos);
		}

		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_loadstring(IntPtr luaState, string chunk);
		
		public static int luaL_dostring(IntPtr luaState, string chunk)
		{
			int result = Lua.luaL_loadstring(luaState, chunk);
			if (result != 0)
				return result;

			return Lua.lua_pcall(luaState, 0, -1, 0);
		}
		
		public static int lua_dostring(IntPtr luaState, string chunk)
		{
			return Lua.luaL_dostring(luaState, chunk);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);
		
		public static void lua_newtable(IntPtr luaState)
		{
			Lua.lua_createtable(luaState, 0, 0);
		}
		
		public static int luaL_dofile(IntPtr luaState, string fileName)
		{
			int result = Lua.luaL_loadfile(luaState, fileName);
			if (result != 0)
				return result;

			return Lua.lua_pcall(luaState, 0, -1, 0);
		}

		public static void lua_getglobal(IntPtr luaState, string name) 
		{
			Lua.lua_pushstring(luaState,name);
			Lua.lua_gettable(luaState,LuaIndexes.LUA_GLOBALSINDEX);
		}

		public static void lua_setglobal(IntPtr luaState, string name)
		{
			Lua.lua_pushstring(luaState,name);
			Lua.lua_insert(luaState,-2);
			Lua.lua_settable(luaState,LuaIndexes.LUA_GLOBALSINDEX);
		}
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_settop(IntPtr luaState, int newTop);
		
		public static void lua_pop(IntPtr luaState, int amount)
		{
			Lua.lua_settop(luaState, -(amount) - 1);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_insert(IntPtr luaState, int newTop);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_remove(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_gettable(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawget(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_settable(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawset(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_setmetatable(IntPtr luaState, int objIndex);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_getmetatable(IntPtr luaState, int objIndex);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_equal(IntPtr luaState, int index1, int index2);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushvalue(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_replace(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_gettop(IntPtr luaState);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern LuaTypes lua_type(IntPtr luaState, int index);
		
		public static bool lua_isnil(IntPtr luaState, int index)
		{
			return (Lua.lua_type(luaState,index)==LuaTypes.LUA_TNIL);
		}
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_isnumber(IntPtr luaState, int index);
		
		public static bool lua_isboolean(IntPtr luaState, int index)
		{
			return Lua.lua_type(luaState,index)==LuaTypes.LUA_TBOOLEAN;
		}
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_ref(IntPtr luaState, int registryIndex);
		
		public static int lua_ref(IntPtr luaState, int lockRef)
		{
			if(lockRef!=0) 
			{
				return Lua.luaL_ref(luaState,LuaIndexes.LUA_REGISTRYINDEX);
			} 
			else return 0;
		}
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr lua_newuserdata(IntPtr luaState, int size);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr lua_touserdata(IntPtr luaState, int index);
		
		public static void lua_getref(IntPtr luaState, int reference)
		{
			Lua.lua_rawgeti(luaState,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);
		
		public static void lua_unref(IntPtr luaState, int reference)
		{
			Lua.luaL_unref(luaState,LuaIndexes.LUA_REGISTRYINDEX,reference);
		}
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_isstring(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_iscfunction(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushnil(IntPtr luaState);

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_call(IntPtr luaState, int nArgs, int nResults);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_pcall(IntPtr luaState, int nArgs, int nResults, int errfunc);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_rawcall(IntPtr luaState, int nArgs, int nResults);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr lua_tocfunction(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern double lua_tonumber(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_toboolean(IntPtr luaState, int index);

		[DllImport(LUADLL,CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr lua_tolstring(IntPtr luaState, int index, out int strLen);

		public static string lua_tostring(IntPtr luaState, int index)
		{
            int strlen;

            IntPtr str = lua_tolstring(luaState, index, out strlen);
            if (str != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(str, strlen);
            else
                return null;            // treat lua nulls to as C# nulls
		}

        [DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void lua_atpanic(IntPtr luaState, LuaFunctionCallback panicf);

		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushnumber(IntPtr luaState, double number);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushboolean(IntPtr luaState, bool value);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushlstring(IntPtr luaState, string str, int size);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushstring(IntPtr luaState, string str);
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_newmetatable(IntPtr luaState, string meta);
		
		// steffenj: BEGIN Lua 5.1.1 API change (luaL_getmetatable is now a macro using lua_getfield)
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_getfield(IntPtr luaState, int stackPos, string meta);
		
		public static void luaL_getmetatable(IntPtr luaState, string meta)
		{
			Lua.lua_getfield(luaState, LuaIndexes.LUA_REGISTRYINDEX, meta);
		}
		
		// steffenj: END Lua 5.1.1 API change (luaL_getmetatable is now a macro using lua_getfield)
		
		[DllImport(LUALIBDLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_checkudata(IntPtr luaState, int stackPos, string meta);
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool luaL_getmetafield(IntPtr luaState, int stackPos, string field);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_load(IntPtr luaState, LuaChunkReader chunkReader, ref ReaderInfo data, string chunkName);
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_loadbuffer(IntPtr luaState, string buff, int size, string name);
		
		[DllImport(LUALIBDLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_loadfile(IntPtr luaState, string filename);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_error(IntPtr luaState);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool lua_checkstack(IntPtr luaState,int extra);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_next(IntPtr luaState,int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_pushlightuserdata(IntPtr luaState, IntPtr udata);
	}
}
