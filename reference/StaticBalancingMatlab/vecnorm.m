function mag = vecnorm(A)
% partial reproduction of new matlab command R2017b
mag = zeros(1,length(A));
for i=1:length(A)
    mag(i) = norm(A(:,i));
end