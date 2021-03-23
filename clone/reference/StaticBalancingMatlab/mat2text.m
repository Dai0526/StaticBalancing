function output = mat2text(fmt,A)
% converts a matrix of numbers to text using sprintf formats fmt. Adds
% del(1) between rows and del(2) between columns
    sz = size(A);
    charss = '[';
    charxe = ']\n[';
    charee = ']';
    charxx = ', ';
    
    str = charss;
    for i=1:sz(1)
        for j=1:sz(2)
            if j==sz(2)
                str = [str fmt charxe];
            else
                str = [str fmt charxx];
            end
        end
    end
    str = [str(1:end-length(charxe)) charee];
    
    output = sprintf(str,A(:));