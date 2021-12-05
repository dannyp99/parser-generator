public interface LexerFactory
{
    simpleLexer GetSimpleLexer();
}

public class CalcFactory : LexerFactory
{
    private string SrcFile { get; set; }
    private string EOF { get; set; }
    public CalcFactory(string srcfile, string eof)
    {
        SrcFile = srcfile;
        EOF = eof;
    }

    public simpleLexer GetSimpleLexer()
    {
        return new CalcLexer(SrcFile, EOF);
    }
}

public class CPlusMinusFactory : LexerFactory
{
    private string SrcFile { get; set; }
    private string EOF { get; set; }

    public CPlusMinusFactory(string srcfile, string eof)
    {
        SrcFile = srcfile;
        EOF = eof;
    }

    public simpleLexer GetSimpleLexer()
    {
        return new CPlusMinusLexer(SrcFile, EOF);
    }
}

public class MongooseFactory : LexerFactory
{
    private string SrcFile { get; set; }
    private string EOF { get; set; }

    public MongooseFactory(string srcfile, string eof)
    {
        SrcFile = srcfile;
        EOF = eof;
    }

    public simpleLexer GetSimpleLexer()
    {
        return new MongooseLexer(SrcFile, EOF);
    }
}