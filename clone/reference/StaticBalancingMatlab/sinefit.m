function s=sinefit(x,y)
% fits a sinasoid with frequency 2*pi through the (x,y) data and returns a
% 3 element vector s that defines that sinasoid as follows:
%       s(1)*cos(x) + s(2)*sin(x) + s(3)

    fit = @(b,x) b(1).*cos(x) + b(2).*sin(x) + b(3); %fitting function
    fcn = @(b) sum((fit(b,x)-y).^2);                 %least-squares cost function
    s = fminsearch(fcn, [0,0,mean(y)]);              %minimize least-squares
end
