using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace TPQuickInfo.SignatureHelp
{
    internal class TypeProviderSignature : ISignature
    {
        private readonly ITextBuffer _textBuffer;
        private IParameter _currentParameter;

        internal TypeProviderSignature(ITextBuffer subjectBuffer, string content, string doc, ReadOnlyCollection<IParameter> parameters)
        {
            _textBuffer = subjectBuffer;
            Content = content;
            Documentation = doc;
            Parameters = parameters;
            _textBuffer.Changed += OnSubjectBufferChanged;
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            CurrentParameterChanged?.Invoke(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter));
        }

        public void ComputeCurrentParameter()
        {
            if (Parameters.Count == 0)
            {
                CurrentParameter = null;
                return;
            }

            //the number of commas in the string is the index of the current parameter
            string sigText = ApplicableToSpan.GetText(_textBuffer.CurrentSnapshot);

            int currentIndex = 0;
            int commaCount = 0;
            while (currentIndex < sigText.Length)
            {
                int commaIndex = sigText.IndexOf(',', currentIndex);
                if (commaIndex == -1)
                {
                    break;
                }
                commaCount++;
                currentIndex = commaIndex + 1;
            }

            if (commaCount < Parameters.Count)
            {
                CurrentParameter = Parameters[commaCount];
            }
            else
            {
                CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        public void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            ComputeCurrentParameter();
        }

        public IParameter CurrentParameter
        {
            get => _currentParameter;
            set
            {
                if (_currentParameter != value)
                {
                    IParameter prevCurrentParameter = _currentParameter;
                    _currentParameter = value;
                    RaiseCurrentParameterChanged(prevCurrentParameter, _currentParameter);
                }
            }
        }
        /// <inheritdoc />
        public ITrackingSpan ApplicableToSpan { get; internal set; }

        /// <inheritdoc />
        public string Content { get; }

        public string Documentation { get; }

        public ReadOnlyCollection<IParameter> Parameters { get; internal set; }

        public string PrettyPrintedContent { get; }

        /// <inheritdoc />
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;
    }
}