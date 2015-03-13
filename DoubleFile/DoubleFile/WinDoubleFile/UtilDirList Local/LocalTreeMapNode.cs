namespace DoubleFile
{
    class LocalTreeMapNode : LocalTreeNode
    {
        internal override string Text { get; set; }

        internal LocalTreeMapNode(string strContent)
            : base()
        {
            Text = strContent;
        }
    }
}
