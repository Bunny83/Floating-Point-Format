using UnityEngine;
using UnityEditor;

public class FloatingPointFormatWindow : EditorWindow
{
    [MenuItem("Tools/B83/FloatingPointFormat")]
	static void Init()
    {
        GetWindow<FloatingPointFormatWindow>();
	}

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public struct FloatHelper
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public float floatVal;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public double doubleVal;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public ulong longVal;

        public ulong fMantissaBits
        {
            get { return longVal & 0x7FFFFFul; }
            set { longVal = (longVal & 0xFFFFFFFFFF800000ul) | (value & 0x7FFFFFul); }
        }
        public byte fExponentBits
        {
            get { return (byte)((longVal >> 23) & 0xFFul); }
            set { longVal = (longVal & 0xFFFFFFFF807FFFFFul) | ((value & 0xFFul) << 23); }
        }
        public sbyte fExponent
        {
            get { return (fExponentBits == 0) ? (sbyte)-126 : (sbyte)(fExponentBits - 127); }
            set { }
        }
        public bool fSignBit
        {
            get { return (longVal & (1ul << 31)) > 0; }
            set { longVal = value ? (longVal | (1ul << 31)) : (longVal & ~(1ul << 31)); }
        }

        public ulong dMantissaBits
        {
            get { return longVal & 0xFFFFFFFFFFFFFul; }
            set { longVal = (longVal & 0xFFF0000000000000ul) | (value & 0xFFFFFFFFFFFFFul); }
        }
        public ushort dExponentBits
        {
            get { return (ushort)((longVal >> 52) & 0x7FFul); }
            set { longVal = (longVal & 0x800FFFFFFFFFFFFFul) | ((value & 0x7FFul) << 52); }
        }
        public short dExponent
        {
            get { return (dExponentBits == 0) ? (short)-1022 : (short)(dExponentBits - 1023); }
            set { }
        }
        public bool dSignBit
        {
            get { return (longVal & (1ul << 63)) > 0; }
            set { longVal = value ? (longVal | (1ul << 63)) : (longVal & ~(1ul << 63)); }
        }


        private static ulong BitButton(ulong aVal, int aBit, Color aCol)
        {
            GUI.color = aCol;
            bool v = (aVal & (1ul << aBit)) > 0;
            if (GUILayout.Toggle(v, "", GUILayout.Width(10)) != v)
            {
                aVal ^= (1ul << aBit);
            }
            return aVal;
        }

        public void DrawFloatGUI()
        {
            var oldCol = GUI.color;

            var tmp = (float)EditorGUILayout.DoubleField(floatVal);
            if (!float.IsNaN(tmp))
                floatVal = tmp;
            GUILayout.BeginHorizontal();
            GUILayout.Label("FloatValue: " + floatVal.ToString("g30"));
            GUILayout.Label("Exp: 2^" + fExponent);
            GUILayout.EndHorizontal();
            // sign bit
            GUILayout.BeginHorizontal();
            longVal = BitButton(longVal, 31, new Color(1, 0.5f, 0.5f, 1f));
            GUILayout.Space(10);
            // exponent bits
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 30; i > 22; i--)
            {
                longVal = BitButton(longVal, i, Color.yellow);
                GUILayout.Space(5.5f);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
                fExponentBits <<= 1;
            if (GUILayout.Button("+"))
                fExponentBits++;
            if (GUILayout.Button("0"))
                fExponentBits = 0;
            if (GUILayout.Button("1"))
                fExponentBits = 0xFF;
            if (GUILayout.Button("-"))
                fExponentBits--;
            if (GUILayout.Button(">"))
                fExponentBits >>= 1;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            // mantissa bits
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 22; i >= 0; i--)
            {
                longVal = BitButton(longVal, i, Color.green);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
                fMantissaBits <<= 1;
            if (GUILayout.Button("+"))
                fMantissaBits++;
            if (GUILayout.Button("0"))
                fMantissaBits = 0;
            if (GUILayout.Button("1"))
                fMantissaBits = 0x7FFFFFul;
            if (GUILayout.Button("-"))
                fMantissaBits--;
            if (GUILayout.Button(">"))
                fMantissaBits >>= 1;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Type: ");
            if (fExponentBits == 0)
            {
                if (fMantissaBits == 0)
                {
                    if (fSignBit)
                        GUILayout.Label("Negative Zero (-0)");
                    else
                        GUILayout.Label("Positive Zero (+0)");
                }
                else
                    GUILayout.Label("Denormalized value");
            }
            else if (fExponentBits == 0xFF)
            {
                if (fMantissaBits == 0)
                {
                    if (fSignBit)
                        GUILayout.Label("Negative Infinity (-inf)");
                    else
                        GUILayout.Label("Positive Infinity (+inf)");
                }
                else if ((fMantissaBits & 0x400000ul) > 0ul)
                    GUILayout.Label("Quiet NaN");
                else
                    GUILayout.Label("Signaling NaN");
            }
            else
                GUILayout.Label("Normalized number");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.color = oldCol;
        }
        public void DrawDoubleGUI()
        {
            var oldCol = GUI.color;
            doubleVal = EditorGUILayout.DoubleField(doubleVal);
            GUILayout.BeginHorizontal();
            GUILayout.Label("DoubleValue: " + doubleVal.ToString("g30"));
            GUILayout.Label("Exp: 2^" + dExponent);
            GUILayout.EndHorizontal();
            // sign bit
            GUILayout.BeginHorizontal();
            longVal = BitButton(longVal, 63, new Color(1, 0.5f, 0.5f, 1f));
            GUILayout.Space(10);
            // exponent bits
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 62; i > 51; i--)
            {
                longVal = BitButton(longVal, i, Color.yellow);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
                dExponentBits <<= 1;
            if (GUILayout.Button("+"))
                dExponentBits++;
            if (GUILayout.Button("0"))
                dExponentBits = 0;
            if (GUILayout.Button("1"))
                dExponentBits = 0x7FF;

            if (GUILayout.Button("-"))
                dExponentBits--;
            if (GUILayout.Button(">"))
                dExponentBits >>= 1;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            // mantissa bits
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 51; i >= 0; i--)
            {
                longVal = BitButton(longVal, i, Color.green);
                if (i == 26)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
                dMantissaBits <<= 1;
            if (GUILayout.Button("+"))
                dMantissaBits++;
            if (GUILayout.Button("0"))
                dMantissaBits = 0;
            if (GUILayout.Button("1"))
                dMantissaBits = 0xFFFFFFFFFFFFFul;
            if (GUILayout.Button("-"))
                dMantissaBits--;
            if (GUILayout.Button(">"))
                dMantissaBits >>= 1;
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Type: ");
            if (dExponentBits == 0)
            {
                if (dMantissaBits == 0)
                {
                    if (dSignBit)
                        GUILayout.Label("Negative Zero (-0)");
                    else
                        GUILayout.Label("Positive Zero (+0)");
                }
                else
                    GUILayout.Label("Denormalized value");
            }
            else if (dExponentBits == 0x7FF)
            {
                if (dMantissaBits == 0)
                {
                    if (dSignBit)
                        GUILayout.Label("Negative Infinity (-inf)");
                    else
                        GUILayout.Label("Positive Infinity (+inf)");
                }
                else if ((dMantissaBits & 0x8000000000000ul) > 0ul)
                    GUILayout.Label("Quiet NaN");
                else
                    GUILayout.Label("Signaling NaN");
            }
            else
                GUILayout.Label("Normalized number");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUI.color = oldCol;
        }
        public void DrawLongGUI()
        {
            var oldCol = GUI.color;
            longVal = (ulong)EditorGUILayout.LongField((long)longVal);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Long signed:" + (long)longVal);
            GUILayout.Label("int signed:" + (int)longVal);
            GUILayout.Label("short signed:" + (short)longVal);
            GUILayout.Label("byte signed:" + (sbyte)longVal);
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Long unsigned:" + longVal);
            GUILayout.Label("int unsigned:" + (uint)longVal);
            GUILayout.Label("short unsigned:" + (ushort)longVal);
            GUILayout.Label("byte unsigned:" + (byte)longVal);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            // upper 32 bits
            GUILayout.BeginHorizontal();
            for (int i = 63; i >= 32; i--)
            {
                longVal = BitButton(longVal, i, new Color(1, 0.5f, 0.5f, 1f));
                if (i % 8 == 0)
                    GUILayout.Space(5);
            }
            GUILayout.EndHorizontal();
            // upper 16 bits of lower 32 bits
            GUILayout.BeginHorizontal();
            for (int i = 31; i >= 16; i--)
            {
                longVal = BitButton(longVal, i, Color.yellow);
                if (i % 8 == 0)
                    GUILayout.Space(5);
            }
            // upper 8 bits of lower 16 bits
            for (int i = 15; i >= 8; i--)
            {
                longVal = BitButton(longVal, i, Color.cyan);
            }
            GUILayout.Space(5);
            // lower 8 bits
            for (int i = 7; i >= 0; i--)
                longVal = BitButton(longVal, i, Color.green);
            GUILayout.EndHorizontal();
            GUI.color = oldCol;
        }
    }

    public FloatHelper val;

    void OnEnable()
    {
        titleContent = new GUIContent("Floating Point Format");
        minSize = new Vector2(600,410);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("", "box");
        GUILayout.Label("Float");
        val.DrawFloatGUI();
        GUILayout.EndVertical();
        GUILayout.BeginVertical("", "box");
        GUILayout.Label("Double");
        val.DrawDoubleGUI();
        GUILayout.EndVertical();
        GUILayout.BeginVertical("", "box");
        GUILayout.Label("Integer");
        val.DrawLongGUI();
        GUILayout.EndVertical();
    }
}
