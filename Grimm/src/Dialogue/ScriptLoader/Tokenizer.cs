
//#define WRITE_DEBUG

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using GameTypes;

namespace GrimmLib
{
	public class Tokenizer
	{
		const string s_lettersThatWorksInNames = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖabcdefghijklmnopqrstuvwxyzåäö_$£@!?";
		const string s_digits = "-1234567890";
		
		List<Token> _tokens;
		TextReader _textReader;
		
		bool _endOfFile;
		char _currentChar;
		int _currentLine;
		int _currentPosition;
		int _currentTokenStartPosition;
		
		public List<Token> process(TextReader pTextReader) {
			D.isNull(pTextReader);

            _tokens = new List<Token>();
            _textReader = pTextReader;
			_endOfFile = false;
            
            readNextChar();            
            _currentLine = 1;
			_currentPosition = 0;
            _currentTokenStartPosition = 0;

			Token t;
			
			do {
				t = readNextToken();
				t.LineNr = _currentLine;
				t.LinePosition = _currentTokenStartPosition;
				_currentTokenStartPosition = _currentPosition;
				
				_tokens.Add(t);
				
#if WRITE_DEBUG
                Console.WriteLine(t.LineNr + ": " + t.getTokenType().ToString() + " " + t.getTokenString());
#endif
				
			} while(t.getTokenType() != Token.TokenType.EOF);
			
			_textReader.Close();
            _textReader.Dispose();
			
			return _tokens;
		}
		
		private Token readNextToken() {
			
			while (!_endOfFile) {
				
				switch(_currentChar) {
                    case '\0':
                        _endOfFile = true;
                        continue;

				case ' ': case '\t':
					readNextChar();
					continue;
					
				case '#':
                    stripComment();
                    continue;
					
				case ':':
					return COLON();
					
				case '.':
					return DOT();
					
				case '\n':
					return NEW_LINE();
					
				case '(':
					return PARANTHESIS_LEFT();
					
				case ')':
					return PARANTHESIS_RIGHT();
					
				case '{':
					return BLOCK_BEGIN();
					
				case '}':
					return BLOCK_END();
					
				case '[':
					return BRACKET_LEFT();
					
				case ']':
					return BRACKET_RIGHT();
					
				case '\"':
					return QUOTED_STRING();
					
				case ',':
					return COMMA();
					
				default:
					if( isLETTER() ) {
						return NAME();
					}
					else if ( isDIGIT() ) {
						return NUMBER(false);
					}                    
                    else
                    {
                        throw new Exception(
                            "Unrecognized character found: \'" +
                            _currentChar + " on line " + _currentLine + 
						    " and position" + _currentPosition);
                    }
				}
				
			}
			
			return new Token(Token.TokenType.EOF, "<EOF>");
		}

        private void stripComment() 
        {
            while (_currentChar != '\n' && _currentChar != '\0')
            {
                readNextChar();
            }
            return;
        }
		
		private Token COLON() {
			readNextChar();
			return new Token(Token.TokenType.COLON, ":");
		}
		
		private Token DOT() {
			readNextChar();
			return new Token(Token.TokenType.DOT, ".");
		}
		
		private Token COMMA() {
			readNextChar();
			return new Token(Token.TokenType.COMMA, ",");
		}
		
		private Token NEW_LINE() {
			while(_currentChar == '\n') { // make several new-lines into a single one
				_currentLine++;
				_currentPosition = 0;
				readNextChar();
			}
			return new Token(Token.TokenType.NEW_LINE, "<NEW_LINE>");
		}
		
		private Token PARANTHESIS_LEFT() {
			readNextChar();
			return new Token(Token.TokenType.PARANTHESIS_LEFT, "(");
		}
		
		private Token PARANTHESIS_RIGHT() {
			readNextChar();
			return new Token(Token.TokenType.PARANTHESIS_RIGHT, ")");
		}
		
		private Token BRACKET_LEFT() {
			readNextChar();
			return new Token(Token.TokenType.BRACKET_LEFT, "[");
		}
		
		private Token BRACKET_RIGHT() {
			readNextChar();
			return new Token(Token.TokenType.BRACKET_RIGHT, "]");
		}
		
		private Token BLOCK_BEGIN() {
			readNextChar();
			return new Token(Token.TokenType.BLOCK_BEGIN, "{");
		}
		
		private Token BLOCK_END() {
			readNextChar();
			return new Token(Token.TokenType.BLOCK_END, "}");
		}
		
		private Token QUOTED_STRING() {
			StringBuilder tokenString = new StringBuilder();
			readNextChar();
            while (_currentChar != '\"' && _currentChar != '\n' && _currentChar != '\0')
            {
				tokenString.Append(_currentChar);
				readNextChar();
			} 
			readNextChar();
			return new Token(Token.TokenType.QUOTED_STRING, tokenString.ToString());
		}
		
		private Token NAME() {
			StringBuilder tokenString = new StringBuilder();
			do {
				tokenString.Append(_currentChar);
				readNextChar();
			} while( isLETTER() || isDIGIT() );
			
			Token.TokenType tokenType = Token.TokenType.NAME;
			
            // Keywords:
			//
			
 			if(tokenString.ToString() == "GOTO") {
				tokenType = Token.TokenType.GOTO;
			}
			else if(tokenString.ToString() == "IF") {
				tokenType = Token.TokenType.IF;
			}
			else if(tokenString.ToString() == "ELSE") {
				tokenType = Token.TokenType.ELSE;
			}
			else if(tokenString.ToString() == "ELIF") {
				tokenType = Token.TokenType.ELIF;
			}
			else if(tokenString.ToString() == "LANGUAGE") {
				tokenType = Token.TokenType.LANGUAGE;
			}
			else if(tokenString.ToString() == "START") {
				tokenType = Token.TokenType.START;
			}
			else if(tokenString.ToString() == "WAIT") {
				tokenType = Token.TokenType.WAIT;
			}
			else if(tokenString.ToString() == "ASSERT") {
				tokenType = Token.TokenType.ASSERT;
			}
			else if(tokenString.ToString() == "LOOP") {
				tokenType = Token.TokenType.LOOP;
			}
			else if(tokenString.ToString() == "STOP") {
				tokenType = Token.TokenType.STOP;
			}
			else if(tokenString.ToString() == "BREAK") {
				tokenType = Token.TokenType.BREAK;
			}
			else if(tokenString.ToString() == "LISTEN") {
				tokenType = Token.TokenType.LISTEN;
			}
			else if(tokenString.ToString() == "BROADCAST") {
				tokenType = Token.TokenType.BROADCAST;
			}
			else if(tokenString.ToString() == "CANCEL") {
				tokenType = Token.TokenType.CANCEL;
			}
			else if(tokenString.ToString() == "FOCUS") {
				tokenType = Token.TokenType.FOCUS;
			}
			else if(tokenString.ToString() == "DEFOCUS") {
				tokenType = Token.TokenType.DEFOCUS;
			}
			else if(tokenString.ToString() == "AND") {
				tokenType = Token.TokenType.AND;
			}
			
			return new Token(tokenType, tokenString.ToString());
		}
		
		private Token NUMBER(bool pNegative) {
			StringBuilder tokenString = new StringBuilder();
			if(pNegative) { tokenString.Append("-"); }
			bool period = false;
			do {
				if (_currentChar == '.' && !period) {
					tokenString.Append(".");
					readNextChar();
				} else if (_currentChar == '.' && period) {
					throw new Exception ("Can't have several period signs in a number!");
				}
				tokenString.Append(_currentChar);
				readNextChar();
			} while( isDIGIT() || _currentChar == '.' );
			
			return new Token(Token.TokenType.NUMBER, tokenString.ToString());
		}
		
		private bool isLETTER() {
			
			foreach(char letter in s_lettersThatWorksInNames) {
				if(_currentChar == letter) return true;
			}
			return false;
		}
		
		private bool isDIGIT() {
			
			foreach(char digit in s_digits) {
				if(_currentChar == digit) return true;
			}
			return false;
		}
		
		void readNextChar() {
			
		    int c = _textReader.Read();
		    if (c > 0) {
		        _currentChar = (char)c;
				_currentPosition++;
			}
		    else {
				_currentChar = '\0';
				_endOfFile = true;
			}
		}	
	}
}