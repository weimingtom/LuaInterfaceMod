using System;
using AT.MIN;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LuaInterface511
{
    public class lua_State
    {
        public IntPtr handler;
    }

    public class CharPtr
    {
        public char[] chars;
        public int index;

        public char this[int offset]
        {
            get { return chars[index + offset]; }
            set { chars[index + offset] = value; }
        }
        public char this[uint offset]
        {
            get { return chars[index + offset]; }
            set { chars[index + offset] = value; }
        }
        public char this[long offset]
        {
            get { return chars[index + (int)offset]; }
            set { chars[index + (int)offset] = value; }
        }

        public static implicit operator CharPtr(string str) { return new CharPtr(str); }
        public static implicit operator CharPtr(char[] chars) { return new CharPtr(chars); }

        public CharPtr()
        {
            this.chars = null;
            this.index = 0;
        }

        public CharPtr(string str)
        {
            this.chars = (str + '\0').ToCharArray();
            this.index = 0;
        }

        public CharPtr(CharPtr ptr)
        {
            this.chars = ptr.chars;
            this.index = ptr.index;
        }

        public CharPtr(CharPtr ptr, int index)
        {
            this.chars = ptr.chars;
            this.index = index;
        }

        public CharPtr(char[] chars)
        {
            this.chars = chars;
            this.index = 0;
        }

        public CharPtr(char[] chars, int index)
        {
            this.chars = chars;
            this.index = index;
        }

        public CharPtr(IntPtr ptr)
        {
            this.chars = new char[0];
            this.index = 0;
        }

        public static CharPtr operator +(CharPtr ptr, int offset) { return new CharPtr(ptr.chars, ptr.index + offset); }
        public static CharPtr operator -(CharPtr ptr, int offset) { return new CharPtr(ptr.chars, ptr.index - offset); }
        public static CharPtr operator +(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index + (int)offset); }
        public static CharPtr operator -(CharPtr ptr, uint offset) { return new CharPtr(ptr.chars, ptr.index - (int)offset); }

        public void inc() { this.index++; }
        public void dec() { this.index--; }
        public CharPtr next() { return new CharPtr(this.chars, this.index + 1); }
        public CharPtr prev() { return new CharPtr(this.chars, this.index - 1); }
        public CharPtr add(int ofs) { return new CharPtr(this.chars, this.index + ofs); }
        public CharPtr sub(int ofs) { return new CharPtr(this.chars, this.index - ofs); }

        public static bool operator ==(CharPtr ptr, char ch) { return ptr[0] == ch; }
        public static bool operator ==(char ch, CharPtr ptr) { return ptr[0] == ch; }
        public static bool operator !=(CharPtr ptr, char ch) { return ptr[0] != ch; }
        public static bool operator !=(char ch, CharPtr ptr) { return ptr[0] != ch; }

        public static CharPtr operator +(CharPtr ptr1, CharPtr ptr2)
        {
            string result = "";
            for (int i = 0; ptr1[i] != '\0'; i++)
                result += ptr1[i];
            for (int i = 0; ptr2[i] != '\0'; i++)
                result += ptr2[i];
            return new CharPtr(result);
        }
        public static int operator -(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index - ptr2.index;
        }
        public static bool operator <(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index < ptr2.index;
        }
        public static bool operator <=(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index <= ptr2.index;
        }
        public static bool operator >(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index > ptr2.index;
        }
        public static bool operator >=(CharPtr ptr1, CharPtr ptr2)
        {
            Debug.Assert(ptr1.chars == ptr2.chars); return ptr1.index >= ptr2.index;
        }
        public static bool operator ==(CharPtr ptr1, CharPtr ptr2)
        {
            object o1 = ptr1 as CharPtr;
            object o2 = ptr2 as CharPtr;
            if ((o1 == null) && (o2 == null)) return true;
            if (o1 == null) return false;
            if (o2 == null) return false;
            return (ptr1.chars == ptr2.chars) && (ptr1.index == ptr2.index);
        }
        public static bool operator !=(CharPtr ptr1, CharPtr ptr2) { return !(ptr1 == ptr2); }

        public override bool Equals(object o)
        {
            return this == (o as CharPtr);
        }

        public override int GetHashCode()
        {
            return 0;
        }
        public override string ToString()
        {
            string result = "";
            for (int i = index; (i < chars.Length) && (chars[i] != '\0'); i++)
                result += chars[i];
            return result;
        }
    }

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
        private const string LUA_PROGNAME = "lua";
        public static CharPtr progname = LUA_PROGNAME;
        public const int LUA_MINSTACK = 20;
        public const int stdout = 0;
        public const int LUA_OK = 0;
        public const int LUA_MULTRET = -1;
        public const int LUA_GCCOLLECT = 2;

        public static CharPtr LUA_QL(CharPtr str)
        {
            return "\"" + str + "\"";
        }

        public static void luai_writestringerror(CharPtr str, params object[] argv)
        {
            string result = Tools.sprintf(str.ToString(), argv);
            Console.Write(result);
        }

        public static int fprintf(int stream, CharPtr str, params object[] argv)
        {
            string result = Tools.sprintf(str.ToString(), argv);
            Console.Write(result);
            return 1;
        }

        public static CharPtr lua_pushfstring(lua_State luaState, CharPtr str, params object[] argv)
        {
            CharPtr result = Tools.sprintf(str.ToString(), argv);
            LuaDLL.lua_pushstring(luaState.handler, result.ToString());
            return result;
        }

        public static int strlen(CharPtr str)
        {
            int index = 0;
            while (str[index] != '\0')
                index++;
            return index;
        }

        public static string luaL_typename(lua_State luaState, int stackPos)
        {
            return LuaDLL.lua_typename(luaState.handler, LuaDLL.lua_type(luaState.handler, stackPos));
        }

        public static lua_State lua_open()
        {
            lua_State luaState = new lua_State();
            luaState.handler = LuaDLL.luaL_newstate();
            return luaState;
        }

        public static int lua_strlen(lua_State luaState, int stackPos)
        {
            return LuaDLL.lua_objlen(luaState.handler, stackPos);
        }

        public static int luaL_dostring(lua_State luaState, string chunk)
        {
            int result = LuaDLL.luaL_loadstring(luaState.handler, chunk);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState.handler, 0, -1, 0);
        }

        //FIXME:
        public static int lua_dostring(lua_State luaState, string chunk)
        {
            return luaL_dostring(luaState, chunk);
        }


        public static void lua_newtable(lua_State luaState)
        {
            LuaDLL.lua_createtable(luaState.handler, 0, 0);
        }

        public static int luaL_dofile(lua_State luaState, string fileName)
        {
            int result = LuaDLL.luaL_loadfile(luaState.handler, fileName);
            if (result != 0)
                return result;

            return LuaDLL.lua_pcall(luaState.handler, 0, -1, 0);
        }

        public static void lua_getglobal(lua_State luaState, string name)
        {
            LuaDLL.lua_pushstring(luaState.handler, name);
            LuaDLL.lua_gettable(luaState.handler, LuaIndexes.LUA_GLOBALSINDEX);
        }

        public static void lua_setglobal(lua_State luaState, string name)
        {
            LuaDLL.lua_pushstring(luaState.handler, name);
            LuaDLL.lua_insert(luaState.handler, -2);
            LuaDLL.lua_settable(luaState.handler, LuaIndexes.LUA_GLOBALSINDEX);
        }

        public static void lua_pop(lua_State luaState, int amount)
        {
            LuaDLL.lua_settop(luaState.handler, -(amount) - 1);
        }

        public static bool lua_isnil(lua_State luaState, int index)
        {
            return LuaDLL.lua_type(luaState.handler, index) == LuaTypes.LUA_TNIL;
        }
        public static bool lua_isboolean(lua_State luaState, int index)
        {
            return LuaDLL.lua_type(luaState.handler, index) == LuaTypes.LUA_TBOOLEAN;
        }
        public static int lua_ref(lua_State luaState, int lockRef)
        {
            if (lockRef != 0)
            {
                return LuaDLL.luaL_ref(luaState.handler, LuaIndexes.LUA_REGISTRYINDEX);
            }
            else return 0;
        }
        public static void lua_getref(lua_State luaState, int reference)
        {
            LuaDLL.lua_rawgeti(luaState.handler, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }

        public static void lua_unref(lua_State luaState, int reference)
        {
            LuaDLL.luaL_unref(luaState.handler, LuaIndexes.LUA_REGISTRYINDEX, reference);
        }
        public static void luaL_getmetatable(lua_State luaState, string meta)
        {
            LuaDLL.lua_getfield(luaState.handler, LuaIndexes.LUA_REGISTRYINDEX, meta);
        }
        public static string lua_tostring(lua_State luaState, int index)
        {
            int strlen;

            IntPtr str = LuaDLL.lua_tolstring(luaState.handler, index, out strlen);
            if (str != IntPtr.Zero)
                return Marshal.PtrToStringAnsi(str, strlen);
            else
                return null; // treat lua nulls to as C# nulls
        }

        public static int lua_gettop(lua_State luaState)
        {
            return LuaDLL.lua_gettop(luaState.handler);
        }

        public static lua_State luaL_newstate() 
        {
            lua_State luaState = new lua_State();
            luaState.handler = LuaDLL.luaL_newstate();
            return luaState;
        }

        public static int lua_pcall(lua_State luaState, int nArgs, int nResults, int errfunc)
        {
            return LuaDLL.lua_pcall(luaState.handler, nArgs, nResults, errfunc);
        }

        public static void luaL_openlibs(lua_State luaState)
        {
            LuaDLL.luaL_openlibs(luaState.handler);
        }

        public static int luaL_loadbuffer(lua_State luaState, string buff, int size, string name)
        {
            return LuaDLL.luaL_loadbuffer(luaState.handler, buff, size, name);
        }
        public static void lua_insert(lua_State luaState, int newTop)
        {
            LuaDLL.lua_insert(luaState.handler, newTop);
        }
        public static int lua_gc(lua_State luaState, LuaGCOptions what, int data)
        {
            return LuaDLL.lua_gc(luaState.handler, what, data);
        }
    }

	public class LuaDLL 
	{
        const string BASEPATH = "";
		const string LUADLL = BASEPATH + "lua511.dll";

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_gc(IntPtr luaState, LuaGCOptions what, int data);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern string lua_typename(IntPtr luaState, LuaTypes type);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_error(IntPtr luaState, string message);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
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
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_callmeta(IntPtr luaState, int stackPos, string name);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_newstate();
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_close(IntPtr luaState);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void luaL_openlibs(IntPtr luaState);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int lua_objlen(IntPtr luaState, int stackPos);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaL_loadstring(IntPtr luaState, string chunk);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_createtable(IntPtr luaState, int narr, int nrec);
	
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_settop(IntPtr luaState, int newTop);
		
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
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool lua_isnumber(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_ref(IntPtr luaState, int registryIndex);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawgeti(IntPtr luaState, int tableIndex, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void lua_rawseti(IntPtr luaState, int tableIndex, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr lua_newuserdata(IntPtr luaState, int size);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern IntPtr lua_touserdata(IntPtr luaState, int index);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern void luaL_unref(IntPtr luaState, int registryIndex, int reference);
		
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
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_newmetatable(IntPtr luaState, string meta);
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern void lua_getfield(IntPtr luaState, int stackPos, string meta);

		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr luaL_checkudata(IntPtr luaState, int stackPos, string meta);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern bool luaL_getmetafield(IntPtr luaState, int stackPos, string field);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int lua_load(IntPtr luaState, LuaChunkReader chunkReader, ref ReaderInfo data, string chunkName);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
		public static extern int luaL_loadbuffer(IntPtr luaState, string buff, int size, string name);
		
		[DllImport(LUADLL,CallingConvention=CallingConvention.Cdecl)]
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
