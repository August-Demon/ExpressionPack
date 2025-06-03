# ExpressionPack
类属性 二进制序列化/反序列化

byte 数组 是纯的数组,没有添加集合长度 开始符 结束符 ,可以直接用来跟Plc 交互

只能用来序列化属性,没有实现字段的序列化

反射 + 表达式树 + 静态泛型类缓存泛型委托 

string  

int、float 、double、 decimal、 long 、ulong 、short 、ushort、 byte 、sbyte、 bool 、char 、DateTime

集合类型 List<> IEnumerable<>、 IReadOnlyCollection<>、 ICollection<> 

字典类型 Dictionary<,>

数组类型 T[ ]

自定义类型

屏蔽属性 [IgnoredMember]
