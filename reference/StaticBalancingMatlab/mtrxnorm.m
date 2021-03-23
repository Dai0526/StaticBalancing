function out=mtrxnorm(in)
% divides each column vector of matrix by its norm and outputs the resulting
% matrix of normalized vectors
    out = zeros(size(in));
    for i=1:length(in)
        out(:,i) = in(:,i)/norm(in(:,i));
    end
end