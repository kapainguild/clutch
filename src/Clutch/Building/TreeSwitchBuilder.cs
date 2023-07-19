using System;
using System.Reflection.Emit;
using Clutch.Building.ProxySupport;

namespace Clutch.Building
{
    class TreeSwitchBuilder
    {
        public static void GenerateTreeBool<T>(ILGenerator g, T[] entries,
            Action<ILGenerator, T, Label> goIfGreater,
            Action<ILGenerator, T, Label> goIfNotEqual,
            Action<ILGenerator, T, Label, Label> doAction,
            int leafLimit = 2)
        {
            GenerateTree(g, entries, goIfGreater, goIfNotEqual, doAction,
                            gen => gen.LoadInteger(1),
                            gen => gen.LoadInteger(0),
                            leafLimit);
        }

        public static void ULongGoIfGreater(ILGenerator g, ulong value, Label label, Action valueLoader)
        {
            valueLoader();
            g.LoadInt64(value);
            g.Emit(OpCodes.Bgt_Un, label);
        }

        public static void ULongGoIfNotEqual(ILGenerator g, ulong value, Label label, Action valueLoader)
        {
            valueLoader();
            g.LoadInt64(value);
            g.Emit(OpCodes.Bne_Un, label);
        }

        public static void GenerateTree<T>(ILGenerator g, T[] entries, 
            Action<ILGenerator, T , Label> goIfGreater, 
            Action<ILGenerator, T, Label> goIfNotEqual,
            Action<ILGenerator, T, Label, Label> doAction,
            Action<ILGenerator> found,
            Action<ILGenerator> notFound,
            int leafLimit = 2)
        {
            var endFound = g.DefineLabel();
            var endNotFound = g.DefineLabel();
            var end = g.DefineLabel();
            GenerateTree(g, entries, 0, entries.Length - 1, goIfGreater, goIfNotEqual, doAction, endFound, endNotFound, leafLimit);

            g.MarkLabel(endNotFound);
            notFound(g);
            g.Goto(end);

            g.MarkLabel(endFound);
            found(g);

            g.MarkLabel(end);
        }

        private static void GenerateTree<T>(ILGenerator g, T[] entries, int left, int right, 
            Action<ILGenerator, T, Label> goIfGreater, 
            Action<ILGenerator, T, Label> goIfNotEqual,
            Action<ILGenerator, T, Label, Label> doAction,
            Label endFound, Label endNotFound, int leafLimit)
        {
            if (right - left <= leafLimit)
            {
                for (int q = left; q <= right; q++)
                {
                    var next = g.DefineLabel();

                    goIfNotEqual(g, entries[q], next);

                    doAction(g, entries[q], endFound, endNotFound);

                    g.MarkLabel(next);
                }
                g.Goto(endNotFound);
            }
            else
            {
                int center = (left + right) / 2;

                var rightSide = g.DefineLabel();

                goIfGreater(g, entries[center], rightSide);

                GenerateTree(g, entries, left, center, goIfGreater, goIfNotEqual, doAction, endFound, endNotFound, leafLimit);

                g.MarkLabel(rightSide);

                GenerateTree(g, entries, center + 1, right, goIfGreater, goIfNotEqual, doAction, endFound, endNotFound, leafLimit);
            }
        }
    }
}
