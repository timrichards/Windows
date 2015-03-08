namespace DoubleFile
{
    class LocalTreeMapNode : LocalTreeNode
    {
        internal new string Text { get; set; }

        internal LocalTreeMapNode(string strContent)
            : base()
        {
            Text = strContent;
        }
    }
}
