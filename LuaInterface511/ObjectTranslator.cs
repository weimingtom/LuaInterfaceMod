using System.Collections.Generic;
using System;


namespace LuaInterface511
{
	public class ObjectTranslator 
	{
        public readonly Dictionary<int, object> objects = new Dictionary<int, object>();
        public readonly Dictionary<object, int> objectsBackMap = new Dictionary<object, int>();
        internal LuaEngine interpreter;

		public ObjectTranslator(LuaEngine interpreter,IntPtr luaState) 
		{
			this.interpreter = interpreter;
			
			createLuaObjectList(luaState);
		}

		private void createLuaObjectList(IntPtr luaState) 
		{
			Lua.lua_pushstring(luaState,"luaNet_objects");
			Lua.lua_newtable(luaState);
			Lua.lua_newtable(luaState);
			Lua.lua_pushstring(luaState,"__mode");
			Lua.lua_pushstring(luaState,"v");
			Lua.lua_settable(luaState,-3);
			Lua.lua_setmetatable(luaState,-2);
			Lua.lua_settable(luaState,LuaIndexes.LUA_REGISTRYINDEX);
		}

		internal void throwError(IntPtr luaState,object e) 
		{
			push(luaState,e);
			Lua.lua_error(luaState);
		}
        
		internal void collectObject(int udata) 
		{
			object o;
            bool found = objects.TryGetValue(udata, out o);

            // The other variant of collectObject might have gotten here first, in that case we will silently ignore the missing entry
            if (found)
            {
                // Debug.WriteLine("Removing " + o.ToString() + " @ " + udata);

                objects.Remove(udata);
                objectsBackMap.Remove(o);
            }
		}

        void collectObject(object o, int udata)
        {
            // Debug.WriteLine("Removing " + o.ToString() + " @ " + udata);

            objects.Remove(udata);
            objectsBackMap.Remove(o);
        }

        int nextObj = 0;

        int addObject(object obj)
        {
            // New object: inserts it in the list
            int index = nextObj++;

            // Debug.WriteLine("Adding " + obj.ToString() + " @ " + index);

            objects[index] = obj;
            objectsBackMap[obj] = index;

            return index;
        }

		internal object getObject(IntPtr luaState,int index) 
		{
			LuaTypes type=Lua.lua_type(luaState,index);
			switch(type) 
			{
				case LuaTypes.LUA_TNUMBER:
				{
					return Lua.lua_tonumber(luaState,index);
				} 
				case LuaTypes.LUA_TSTRING: 
				{
					return Lua.lua_tostring(luaState,index);
				} 
				case LuaTypes.LUA_TBOOLEAN:
				{
					return Lua.lua_toboolean(luaState,index);
				} 
				case LuaTypes.LUA_TTABLE: 
				{
					return getTable(luaState,index);
				} 
				case LuaTypes.LUA_TFUNCTION:
				{
					return getFunction(luaState,index);
				} 
				case LuaTypes.LUA_TUSERDATA:
				{
					throw new Exception("getObject");
					return null; //FIXME:
				}
				default:
					return null;
			}
		}

		internal LuaTable getTable(IntPtr luaState,int index) 
		{
			Lua.lua_pushvalue(luaState,index);
			return new LuaTable(Lua.lua_ref(luaState,1),interpreter);
		}

		internal LuaUserData getUserData(IntPtr luaState,int index) 
		{
			Lua.lua_pushvalue(luaState,index);
			return new LuaUserData(Lua.lua_ref(luaState,1),interpreter);
		}

		internal LuaFunction getFunction(IntPtr luaState,int index) 
		{
			Lua.lua_pushvalue(luaState,index);
			return new LuaFunction(Lua.lua_ref(luaState,1),interpreter);
		}

		internal int returnValues(IntPtr luaState, object[] returnValues) 
		{
			if(Lua.lua_checkstack(luaState,returnValues.Length+5)) 
			{
				for(int i=0;i<returnValues.Length;i++) 
				{
					push(luaState,returnValues[i]);
				}
				return returnValues.Length;
			} else
				return 0;
		}

		internal object[] popValues(IntPtr luaState, int oldTop) 
		{
			int newTop = Lua.lua_gettop(luaState);
			if (oldTop == newTop) 
			{
				return null;
			} 
			else 
			{
				if (newTop - oldTop > 0)
				{
					object[] returnValues = new object[newTop - oldTop];
					for (int i = oldTop + 1, index = 0; i <= newTop; i++) 
					{
						returnValues[index++] = getObject(luaState, i);
					}
					Lua.lua_settop(luaState, oldTop);
					return returnValues;
				}
				else
				{
					return new object[0];
				}
			}
		}

		internal void push(IntPtr luaState, object o) 
		{
			if(o==null) 
			{
				Lua.lua_pushnil(luaState);
			}
			else if(o is sbyte || o is byte || o is short || o is ushort ||
				o is int || o is uint || o is long || o is float ||
				o is ulong || o is decimal || o is double) 
			{
				double d=Convert.ToDouble(o);
				Lua.lua_pushnumber(luaState,d);
			}
			else if(o is char)
			{
				double d = (char)o;
				Lua.lua_pushnumber(luaState,d);
			}
			else if(o is string)
			{
				string str=(string)o;
				Lua.lua_pushstring(luaState,str);
			}
			else if(o is bool)
			{
				bool b=(bool)o;
				Lua.lua_pushboolean(luaState,b);
			}
			else if(o is LuaTable) 
			{
				((LuaTable)o).push(luaState);
			} 
			else if(o is LuaCSFunction) 
			{
				throw new Exception();
			} 
			else if(o is LuaFunction)
			{
				((LuaFunction)o).push(luaState);
			}
			else 
			{
				throw new Exception();
			}
		}
	}
}