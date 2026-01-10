Same as select but with different api, T is a IEnumerable<T> / List<T> and you always work with multiple values.

Should integrate with EditForm and validation.

<MultiSelectRoot T="List<ComplexObject>" @bind-Value="selectedItems">
    <MultiSelectTrigger>
        <MultiSelectedItems> // <-- you can place this where ever you want inside the root
            <Template Context="item"> // <-- item is of type T
                <Badge>
                    @item.Name
                </Badge>
            </Template>
        </MultiSelectedItems>
    </MultiSelectTrigger>
    --- content ---
</MultiSelectRoot> 


The items has properties like Checked / Selected so ones can style however they want.
Maybe use inheritance from Select component as a lot is crossing here.