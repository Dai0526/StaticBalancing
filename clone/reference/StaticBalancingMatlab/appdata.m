function output = appdata(varargin)
% Provides several options for interacting with apdata
% val = appdata(h,'get','var_name')
%   first argument is the handle of object being accessed. If the first argument
%   is not a handle, it is assumed to be "0"
%
%   second argument defines the operation.
%       'get' - retreives a specific value. Arguments must include variable name as string
%               ex: val = appdata(h,'get','var_name')
%       'set' - stores a specific value. Arguments are typically variable
%           name as string followed by value. If only 1 argument is provided it
%           will be used both as the name and value. There is an option to
%           provie an index argument of 'yes' or 'no' defining whether the
%           new value will be added to an index of files for bulk
%           operations (like saving or clearing). The default is 'yes'
%               ex: appdata(h,'set','config',value,index)
%                   appdata('set',config)
%      'load' - opens a .mat file and loads all values into app data
%               ex: appata(h,'load','C:\files\config.mat')
%      'save' - saves all indexed variables into a .mat file. Option to
%           include excemptions that you do not want to be save.
%               ex: appdata(h,'save','C:\files\config.mat',{'dontsaveme'})
%     'clear' - delets all indexed variables from appdata. Option to
%           include excemptions that you do not want deleted.


p = 1; %pointer
if isa(varargin{p},'function_handle')
    h=varagin{p};
    p = p+1;
else
    h=0;
end

switch varargin{p}
    case 'get'
        name = varargin{p+1};
        output = getappdata(h,name);
        return
        
    case 'set'
        reargin = varargin(p+1:length(varargin));
        renargin = length(reargin);
        
        if renargin == 1
            name = inputname(p+1);
            value = varargin{p+1};
            setappdata(h,name,value);
            index(h,name);
            return        
        elseif renargin == 3
            name = reargin{1};
            value = reargin{2};
            setappdata(h,name,value);
            if strcmpi(reargin{3},'yes')
                index(h,name);
            end
            return
        else %renargin == 2
            if ischar(reargin{2})
                if strcmpi(reargin{2},'yes')
                    name = inputname(p+1);
                    value = varargin{p+1};
                    setappdata(h,name,value);
                    index(h,name);
                    return
                elseif strcmpi(reargin{2},'no')
                    name = inputname(p+1);
                    value = varargin{p+1};
                    setappdata(h,name,value);
                    return
                end
            end
                name = reargin{1};
                value = reargin{2};
                setappdata(h,name,value);
                index(h,name);
                return
        end
        
    case 'load'
        filepath = varargin{p+1};
        import(h,filepath);
        return
        
    case 'save'
        filepath = varargin{p+1};
        if nargin> p+1
            exempts = varargin{p+2};
        else
            exempts = {' '};
        end
        export(h,filepath,exempts)
        
    case 'clear'
        if nargin > p
            exempts = varargin{p+1};
        else
            exempts = {' '};
        end
        remove(h,exempts)
end

function store(h,name,value,ind)
setappdata(h,name,value);
if ind
    index(h,name);
end
      
function import(h,filepath)
data = load(filepath);
fields = fieldnames(data);
for i=1:length(fields)
    setappdata(h,fields{i},getfield(data,fields{i}));
end
setappdata(h,'capturedfieldnames',fields);

function export(h,filepath,exempts)
fields = getappdata(h,'capturedfieldnames');
if isnumeric(h)
    h = num2str(h);
end
for i=1:length(fields)
    if any(strcmp(exempts,fields{i}))
        continue
    end
    cmd = [fields{i} ' = getappdata(' h ',''' fields{i} ''');'];
    eval(cmd);
end
clear h fields i cmd
save(filepath);

function remove(h,exempts)
fields = getappdata(h,'capturedfieldnames');
for i = 1:length(fields)
    if any(strcmp(exempts,fields{i}))
        continue
    end
    rmappdata(h,fields{i});
end

function index(h,name)
fields = getappdata(h,'capturedfieldnames');
if isempty(fields)
    fields{1} = 'capturedfieldnames';
end
if any(strcmp(name,fields))
    return
else
    fields{length(fields)+1} = name;
end
setappdata(h,'capturedfieldnames',fields);

                        
                        