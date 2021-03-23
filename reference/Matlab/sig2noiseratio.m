function SNR = sig2noiseratio(datay,signaly)
%calculates the amplitude based (not power based) signal-to-noise ratio of
%the xy-data with respect to the defined xy-signal data
% x must be uniformyly spaced

% is this mathmatically correct???
noisey = datay-signaly;
rms = @(x) sqrt(1/length(x)*sum(x.^2));

signalrms = rms(signaly);
noiserms = rms(noisey);

SNR = 10*log10(signalrms/noiserms);
