using System;
using GameTypes;

namespace GrimmLib
{
	public class Token
	{
		public enum TokenType { 
			
			NO_TOKEN_TYPE, 
			EOF, 
            NEW_LINE, 
            COMMA,
			
            NAME,
            NUMBER, 
            QUOTED_STRING,
			SWITCH,
			COLON,
			END,
			GOTO,
			
            PARANTHESIS_LEFT, 
            PARANTHESIS_RIGHT, 
			BLOCK_BEGIN, 
            BLOCK_END,
			BRACKET_LEFT,
			BRACKET_RIGHT,
			DOT,
			
			IF,
			ELSE,
			ELIF,
			
			CHOICE,
			LANGUAGE,
			START,
			INTERRUPT,
			WAIT,
			ASSERT,
			LOOP,
			BREAK,
			STOP,
			LISTEN,
			BROADCAST,
			CANCEL,
			FOCUS,
			DEFOCUS,
			AND,
			
			ETERNAL
			
		};
		
		TokenType _tokenType;
		string _tokenString;
		int _lineNr = -1;
		int _linePosition = -1;
		
		public Token (TokenType pTokenType, string pTokenString)
		{
			_tokenType = pTokenType;
			_tokenString = pTokenString;
		}

        public Token(TokenType pTokenType, string pTokenString, int pLineNr, int pLinePosition)
        {
            _tokenType = pTokenType;
            _tokenString = pTokenString;
            _lineNr = pLineNr;
            _linePosition = pLinePosition;
        }
		
		public TokenType getTokenType() { return _tokenType; }
		public string getTokenString() { return _tokenString; }
		
		public int LineNr {
			set {
				_lineNr = value;
			}
			get {
				return _lineNr;
			}
		}
		
		public int LinePosition {
			set {
				_linePosition = value;
			}
			get {
				return _linePosition;
			}
		}
	}
}

